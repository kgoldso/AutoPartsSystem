// Models/User.cs
namespace AutoPartsSystem.Models;

/// <summary>
/// Пользователь системы (Сотрудник или зарегистрированный Клиент).
/// </summary>
public class User
{
    public int Id { get; set; }
    
    /// <summary>Логин пользователя (уникальный).</summary>
    public string Login { get; set; } = string.Empty;
    
    /// <summary>Хэш пароля (в рамках курсовой пока храним просто строку).</summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>Роль в системе: Admin, Manager, Warehouse, Client.</summary>
    public string Role { get; set; } = string.Empty; 
}