using System;
using System.Collections.Generic;
using System.Linq;
using AutoPartsSystem.Data;
using AutoPartsSystem.Models;

namespace AutoPartsSystem.Services;

/// <summary>
/// Сервис для управления заказами и расчетами стоимости/сроков.
/// </summary>
public class OrderService
{
    private readonly IRepository _repository;

    public OrderService(IRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Оформляет новый заказ, применяя бизнес-правила срочности и обновляя склад.
    /// </summary>
    /// <param name="email">Email клиента.</param>
    /// <param name="fullName">ФИО клиента (используется, если клиент новый).</param>
    /// <param name="phone">Телефон клиента (используется, если клиент новый).</param>
    /// <param name="partId">ID запчасти.</param>
    /// <param name="quantity">Количество.</param>
    /// <param name="isUrgent">Флаг срочности заказа.</param>
    /// <returns>Оформленный заказ.</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается, если товара недостаточно на складе.</exception>
    /// <exception cref="ArgumentException">Выбрасывается, если деталь не найдена.</exception>
    public Order PlaceOrder(string email, string fullName, string phone, int partId, int quantity, bool isUrgent) // TODO: Проверяет наличие товара на складе
    // Автоматический ReserveParts, Бизнес-правила, Работа с клиентом
    {
        if (quantity <= 0)
            throw new ArgumentException("Количество должно быть больше нуля.", nameof(quantity));

        // 1. Поиск или создание клиента
        var customer = _repository.FindCustomerByEmail(email);
        if (customer == null)
        {
            customer = new Customer { Email = email, FullName = fullName, Phone = phone };
            _repository.AddCustomer(customer);
        }

        // 2. Поиск детали и проверка остатков
        var part = _repository.GetPartById(partId) 
            ?? throw new ArgumentException($"Деталь с ID {partId} не найдена.");

        if (part.Stock < quantity)
            throw new InvalidOperationException($"Недостаточно товара на складе. В наличии: {part.Stock}, запрошено: {quantity}.");

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
            CustomerId = customer.Id,
            PartId = part.Id,
            Quantity = quantity,
            TotalPrice = totalPrice,
            Urgent = isUrgent,
            Status = "Новый",
            DeliveryMethod = "Доставка", // Можно вынести в параметры метода
            OrderDate = DateTime.Now,
            EstimatedDeliveryDate = DateTime.Now.AddDays(leadTime)
        };

        // 5. Сохранение изменений
        // Списываем товар со склада
        _repository.UpdatePartStock(part.Id, part.Stock - quantity);
        // Сохраняем заказ
        _repository.AddOrder(order);

        return order;
    }

    /// <summary>
    /// Возвращает историю заказов для указанного клиента.
    /// </summary>
    public List<Order> GetOrderHistory(int customerId) // TODO: Метод фильтрует все заказы из репозитория по CustomerId и сортирует их по дате.
    {
        return _repository.GetAllOrders()
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();
    }
}