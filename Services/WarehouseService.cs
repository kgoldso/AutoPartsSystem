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
    private readonly IdentityService _identityService; // Добавили зависимость

    public WarehouseService(IRepository repository, IdentityService identityService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }

    /// <summary>
    /// Регистрирует отгрузку заказа, переводя его в статус 'Отгружен'.
    /// </summary>
    public void RegisterShipment(int orderId) // TODO: Переводит заказ в статус «Отгружен». Содержит проверки: нельзя отгрузить уже отгруженный или отмененный заказ. Защищен проверкой роли Warehouse
    {
        _identityService.EnsureRole("Warehouse"); // Защита метода

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
    public List<Order> GetPendingShipments() // TODO: Возвращает список заказов со статусами «Новый» или «В обработке» для работы кладовщика.
    {
        _identityService.EnsureRole("Warehouse"); // Защита метода

        return _repository.GetAllOrders()
            .Where(o => o.Status == "Новый" || o.Status == "В обработке")
            .OrderBy(o => o.OrderDate)
            .ToList();
    }

    /// <summary>
    /// Обновление остатков на складе (оприходование новой партии).
    /// </summary>
    public void UpdateInventory(int partId, int quantityAdded) // TODO: Позволяет оприходовать новую партию товара, увеличивая значение Stock в базе данных.
    {
        _identityService.EnsureRole("Warehouse"); // Только Кладовщик (и Админ) может делать это

        if (quantityAdded <= 0)
            throw new ArgumentException("Добавляемое количество должно быть больше нуля.");

        var part = _repository.GetPartById(partId) 
            ?? throw new ArgumentException($"Деталь с ID {partId} не найдена.");

        _repository.UpdatePartStock(partId, part.Stock + quantityAdded);
    }
}