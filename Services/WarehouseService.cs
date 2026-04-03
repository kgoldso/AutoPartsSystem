using AutoPartsSystem.Data;
using AutoPartsSystem.Models;

namespace AutoPartsSystem.Services;

/// <summary>
/// Сервис для работы склада и управления отгрузками.
/// Использует Result Pattern и асинхронность.
/// </summary>
public class WarehouseService(IRepository repository, IdentityService identityService)
{
    /// <summary>
    /// Регистрирует отгрузку заказа, переводя его в статус 'Отгружен'.
    /// </summary>
    public async Task<Result> RegisterShipmentAsync(int orderId)
    {
        var roleCheck = identityService.EnsureRole("Warehouse");
        if (!roleCheck.IsSuccess) return roleCheck;

        var order = await repository.GetOrderByIdAsync(orderId);
        if (order == null)
            return Result.Failure($"Заказ с ID {orderId} не найден.");

        if (order.Status.Equals("Отменен", StringComparison.OrdinalIgnoreCase))
            return Result.Failure("Нельзя отгрузить отмененный заказ.");

        if (order.Status.Equals("Отгружен", StringComparison.OrdinalIgnoreCase))
            return Result.Failure("Заказ уже был отгружен ранее.");

        await repository.UpdateOrderStatusAsync(orderId, "Отгружен");
        return Result.Success();
    }

    /// <summary>
    /// Получает список заказов, готовых к обработке или сборке.
    /// </summary>
    public async Task<Result<List<Order>>> GetPendingShipmentsAsync()
    {
        var roleCheck = identityService.EnsureRole("Warehouse");
        if (!roleCheck.IsSuccess) return Result<List<Order>>.Failure(roleCheck.Error!);

        var allOrders = await repository.GetAllOrdersAsync();
        var pending = allOrders
            .Where(o => o.Status == "Новый" || o.Status == "В обработке")
            .OrderBy(o => o.OrderDate)
            .ToList();
        
        return Result<List<Order>>.Success(pending);
    }

    /// <summary>
    /// Обновление остатков на складе (оприходование новой партии).
    /// </summary>
    public async Task<Result> UpdateInventoryAsync(int partId, int quantityAdded)
    {
        var roleCheck = identityService.EnsureRole("Warehouse");
        if (!roleCheck.IsSuccess) return roleCheck;

        if (quantityAdded <= 0)
            return Result.Failure("Добавляемое количество должно быть больше нуля.");

        var part = await repository.GetPartByIdAsync(partId);
        if (part == null)
            return Result.Failure($"Деталь с ID {partId} не найдена.");

        await repository.UpdatePartStockAsync(partId, part.Stock + quantityAdded);
        return Result.Success();
    }
}
