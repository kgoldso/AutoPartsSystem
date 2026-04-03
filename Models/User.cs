namespace AutoPartsSystem.Models;

/// <summary>
/// Пользователь системы (Сотрудник или зарегистрированный Клиент).
/// </summary>
public class User
{
    public int Id { get; set; }
    
    /// <summary>Логин пользователя (уникальный).</summary>
    public required string Login { get; set; }
    
    /// <summary>Хэш пароля.</summary>
    public required string PasswordHash { get; set; }
    
    /// <summary>Роль в системе: Admin, Manager, Warehouse, Client.</summary>
    public required string Role { get; set; } 
}
