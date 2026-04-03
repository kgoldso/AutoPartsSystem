using AutoPartsSystem.Data;
using AutoPartsSystem.Models;

namespace AutoPartsSystem.Services;

/// <summary>
/// Сервис для управления заказами и расчетами стоимости/сроков.
/// Использует Result Pattern и асинхронность (C# 12).
/// </summary>
public class OrderService(IRepository repository)
{
    /// <summary>
    /// Оформляет новый заказ, применяя бизнес-правила срочности и обновляя склад.
    /// </summary>
    public async Task<Result<Order>> PlaceOrderAsync(int userId, string email, string fullName, string phone, int partId, int quantity, bool isUrgent)
    {
        if (quantity <= 0)
            return Result<Order>.Failure("Количество должно быть больше нуля.");

        // 1. Поиск или создание клиента
        var customer = await repository.FindCustomerByEmailAsync(email);
        if (customer == null)
        {
            customer = new Customer { Email = email, FullName = fullName, Phone = phone };
            await repository.AddCustomerAsync(customer);
        }
        else
        {
            // Если данные изменились, обновляем профиль клиента (кроме Email)
            if (customer.FullName != fullName || customer.Phone != phone)
            {
                customer.FullName = fullName;
                customer.Phone = phone;
                await repository.UpdateCustomerAsync(customer);
            }
        }

        // 2. Поиск детали и проверка остатков
        var part = await repository.GetPartByIdAsync(partId);
        if (part == null)
            return Result<Order>.Failure($"Деталь с ID {partId} не найдена.");

        if (part.Stock < quantity)
            return Result<Order>.Failure($"Недостаточно товара на складе. В наличии: {part.Stock}, запрошено: {quantity}.");

        // 3. Применение бизнес-правил
        decimal basePrice = part.Price * quantity;
        decimal totalPrice = isUrgent ? basePrice * 1.2m : basePrice;

        int leadTime = part.LeadTimeDays;
        if (isUrgent)
        {
            leadTime = Math.Max(1, leadTime - 2);
        }

        // 4. Формирование заказа
        var order = new Order
        {
            UserId = userId,
            CustomerId = customer.Id,
            CustomerFullName = fullName,
            CustomerEmail = email,
            CustomerPhone = phone,
            PartId = part.Id,
            Quantity = quantity,
            TotalPrice = totalPrice,
            Urgent = isUrgent,
            Status = "Новый",
            DeliveryMethod = "Доставка",
            OrderDate = DateTime.Now,
            EstimatedDeliveryDate = DateTime.Now.AddDays(leadTime)
        };

        // 5. Сохранение изменений
        await repository.UpdatePartStockAsync(part.Id, part.Stock - quantity);
        await repository.AddOrderAsync(order);

        return Result<Order>.Success(order);
    }

    /// <summary>
    /// Отменяет заказ и возвращает товар на склад.
    /// </summary>
    public async Task<Result<bool>> CancelOrderAsync(int orderId)
    {
        var order = await repository.GetOrderByIdAsync(orderId);
        if (order == null)
            return Result<bool>.Failure("Заказ не найден.");

        if (order.Status is "Отменен" or "Отменён")
            return Result<bool>.Failure("Заказ уже отменен.");

        if (order.Status == "Отгружен")
            return Result<bool>.Failure("Нельзя отменить отгруженный заказ.");

        // 1. Возвращаем товар на склад
        var part = await repository.GetPartByIdAsync(order.PartId);
        if (part != null)
        {
            await repository.UpdatePartStockAsync(part.Id, part.Stock + order.Quantity);
        }

        // 2. Обновляем статус заказа
        await repository.UpdateOrderStatusAsync(orderId, "Отменен");

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Возвращает историю заказов для указанного клиента.
    /// </summary>
    public async Task<List<Order>> GetOrderHistoryAsync(int customerId)
    {
        var allOrders = await repository.GetAllOrdersAsync();
        return allOrders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();
    }
}
