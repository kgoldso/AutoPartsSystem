using System;
using System.Collections.Generic;
using System.Linq;
using AutoPartsSystem.Data;
using AutoPartsSystem.Models;

namespace AutoPartsSystem.Services;

/// <summary>
/// Сервис для административных задач и аналитики.
/// </summary>
public class AdminService
{
    private readonly IRepository _repository;

    public AdminService(IRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Генерирует отчет по запчастям, остаток которых ниже критического уровня.
    /// </summary>
    /// <param name="criticalThreshold">Критический порог остатка (по умолчанию 5).</param>
    public List<Part> GenerateStockReport(int criticalThreshold = 5)
    {
        return _repository.GetAllParts()
            .Where(p => p.Stock < criticalThreshold)
            .OrderBy(p => p.Stock)
            .ToList();
    }

    /// <summary>
    /// Рассчитывает общую сумму выручки по успешно отгруженным заказам.
    /// </summary>
    public decimal GenerateSalesReport()
    {
        return _repository.GetAllOrders()
            .Where(o => o.Status.Equals("Отгружен", StringComparison.OrdinalIgnoreCase))
            .Sum(o => o.TotalPrice);
    }
}