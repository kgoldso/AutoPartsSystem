using System;
using System.Collections.Generic;
using System.Linq;
using AutoPartsSystem.Data;
using AutoPartsSystem.Models;

namespace AutoPartsSystem.Services;

/// <summary>
/// Сервис для работы склада и управления отгрузками.
/// </summary>
public class WarehouseService
{
    private readonly IRepository _repository;

    public WarehouseService(IRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Регистрирует отгрузку заказа, переводя его в статус 'Отгружен'.
    /// </summary>
    /// <param name="orderId">ID заказа.</param>
    /// <exception cref="InvalidOperationException">Если заказ отменен или уже отгружен.</exception>
    /// <exception cref="ArgumentException">Если заказ не найден.</exception>
    public void RegisterShipment(int orderId)
    {
        var order = _repository.GetOrderById(orderId) 
            ?? throw new ArgumentException($"Заказ с ID {orderId} не найден.");

        if (order.Status.Equals("Отменен", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Нельзя отгрузить отмененный заказ.");

        if (order.Status.Equals("Отгружен", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Заказ уже был отгружен ранее.");

        _repository.UpdateOrderStatus(orderId, "Отгружен");
    }

    /// <summary>
    /// Получает список заказов, готовых к обработке или сборке.
    /// </summary>
    public List<Order> GetPendingShipments()
    {
        return _repository.GetAllOrders()
            .Where(o => o.Status == "Новый" || o.Status == "В обработке")
            .OrderBy(o => o.OrderDate)
            .ToList();
    }
}