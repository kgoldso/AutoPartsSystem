using System;
using AutoPartsSystem.Data;
using AutoPartsSystem.Models;

namespace AutoPartsSystem.Services;

/// <summary>
/// Сервис аутентификации и авторизации (RBAC).
/// </summary>
public class IdentityService
{
    private readonly IRepository _repository;
    
    /// <summary>Текущий авторизованный пользователь. Если null — значит это Гость.</summary>
    public User? CurrentUser { get; private set; }

    public IdentityService(IRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>Попытка входа в систему.</summary>
    public bool Login(string login, string password) // TODO: Аутентификация. Она ищет пользователя в базе через репозиторий и проверяет совпадение пароля.
    {
        var user = _repository.GetUserByLogin(login);
        if (user == null) return false;

        // В реальном проекте здесь используется BCrypt.Verify, 
        // но для курсовой мы просто сравниваем строки.
        if (user.PasswordHash == password)
        {
            CurrentUser = user;
            return true;
        }
        return false;
    }

    /// <summary>Выход из системы.</summary>
    public void Logout()
    {
        CurrentUser = null;
    }

    /// <summary>
    /// Проверка прав доступа. Выбрасывает исключение, если прав нет.
    /// </summary>
    /// <param name="requiredRole">Требуемая роль (например, 'Admin' или 'Manager').</param>
    public void EnsureRole(string requiredRole) // TODO: Авторизация. Проверяет, авторизован ли пользователь и соответствует ли его роль требуемой
    {
        if (CurrentUser == null)
            throw new UnauthorizedAccessException("Отказ в доступе: Пользователь не авторизован.");
        
        // Админ имеет доступ ко всему, остальные - только к своей роли
        if (CurrentUser.Role != "Admin" && CurrentUser.Role != requiredRole)
            throw new UnauthorizedAccessException($"Отказ в доступе: Требуется роль '{requiredRole}', а у вас '{CurrentUser.Role}'.");
    }
}