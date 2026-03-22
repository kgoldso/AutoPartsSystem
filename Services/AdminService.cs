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
    private readonly IdentityService _identityService; // Добавили зависимость

    // Обновляем конструктор
    public AdminService(IRepository repository, IdentityService identityService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }

    /// <summary>
    /// Генерирует отчет по запчастям, остаток которых ниже критического уровня.
    /// Требует роль Admin.
    /// </summary>
    public List<Part> GenerateStockReport(int criticalThreshold = 5)
    {
        _identityService.EnsureRole("Admin"); // Защита метода!

        return _repository.GetAllParts()
            .Where(p => p.Stock < criticalThreshold)
            .OrderBy(p => p.Stock)
            .ToList();
    }

    /// <summary>
    /// Рассчитывает общую сумму выручки по успешно отгруженным заказам.
    /// Требует роль Admin.
    /// </summary>
    public decimal GenerateSalesReport()
    {
        _identityService.EnsureRole("Admin"); // Защита метода!

        return _repository.GetAllOrders()
            .Where(o => o.Status.Equals("Отгружен", StringComparison.OrdinalIgnoreCase))
            .Sum(o => o.TotalPrice);
    }

    /// <summary>
    /// Управление пользователями: Регистрация нового сотрудника.
    /// </summary>
    public User RegisterEmployee(string login, string password, string role)
    {
        _identityService.EnsureRole("Admin"); // Только Админ может создавать сотрудников

        // Проверяем, существует ли уже такой логин
        if (_repository.GetUserByLogin(login) != null)
            throw new InvalidOperationException($"Пользователь с логином {login} уже существует.");

        var newUser = new User 
        { 
            Login = login, 
            PasswordHash = password, 
            Role = role 
        };
        
        _repository.AddUser(newUser);
        return newUser;
    }
}