using AutoPartsSystem.Data;
using AutoPartsSystem.Models;

namespace AutoPartsSystem.Services;

/// <summary>
/// Сервис аутентификации и авторизации (RBAC).
/// Использует Result Pattern и асинхронность.
/// </summary>
public class IdentityService(IRepository repository)
{
    /// <summary>Текущий авторизованный пользователь. Если null — значит это Гость.</summary>
    public User? CurrentUser { get; private set; }

    /// <summary>Попытка входа в систему.</summary>
    public async Task<Result<User>> LoginAsync(string login, string password)
    {
        var user = await repository.GetUserByLoginAsync(login);
        if (user == null) 
            return Result<User>.Failure("Пользователь не найден.");

        if (user.PasswordHash == password)
        {
            CurrentUser = user;
            return Result<User>.Success(user);
        }
        
        return Result<User>.Failure("Неверный пароль.");
    }

    /// <summary>Выход из системы.</summary>
    public void Logout() => CurrentUser = null;

    /// <summary>
    /// Проверка прав доступа.
    /// </summary>
    public Result EnsureRole(string requiredRole)
    {
        if (CurrentUser == null)
            return Result.Failure("Отказ в доступе: Пользователь не авторизован.");
        
        if (CurrentUser.Role != "Admin" && CurrentUser.Role != requiredRole)
            return Result.Failure($"Отказ в доступе: Требуется роль '{requiredRole}', а у вас '{CurrentUser.Role}'.");

        return Result.Success();
    }
}
