using AutoPartsSystem.Data;
using AutoPartsSystem.Models;

namespace AutoPartsSystem.Services;

/// <summary>
/// Сервис для административных задач и аналитики.
/// Использует Result Pattern и асинхронность.
/// </summary>
public class AdminService(IRepository repository, IdentityService identityService)
{
    /// <summary>
    /// Генерирует отчет по запчастям, остаток которых ниже критического уровня.
    /// </summary>
    public async Task<Result<List<Part>>> GenerateStockReportAsync(int criticalThreshold = 5)
    {
        var roleCheck = identityService.EnsureRole("Admin");
        if (!roleCheck.IsSuccess) return Result<List<Part>>.Failure(roleCheck.Error!);

        var allParts = await repository.GetAllPartsAsync();
        var report = allParts
            .Where(p => p.Stock < criticalThreshold)
            .OrderBy(p => p.Stock)
            .ToList();
        
        return Result<List<Part>>.Success(report);
    }

    /// <summary>
    /// Рассчитывает общую сумму выручки по успешно отгруженным заказам.
    /// </summary>
    public async Task<Result<decimal>> GenerateSalesReportAsync()
    {
        var roleCheck = identityService.EnsureRole("Admin");
        if (!roleCheck.IsSuccess) return Result<decimal>.Failure(roleCheck.Error!);

        var allOrders = await repository.GetAllOrdersAsync();
        var totalSales = allOrders
            .Where(o => o.Status.Equals("Отгружен", StringComparison.OrdinalIgnoreCase))
            .Sum(o => o.TotalPrice);
            
        return Result<decimal>.Success(totalSales);
    }

    /// <summary>
    /// Управление пользователями: Регистрация нового сотрудника.
    /// </summary>
    public async Task<Result<User>> RegisterEmployeeAsync(string login, string password, string role)
    {
        var roleCheck = identityService.EnsureRole("Admin");
        if (!roleCheck.IsSuccess) return Result<User>.Failure(roleCheck.Error!);

        if (await repository.GetUserByLoginAsync(login) != null)
            return Result<User>.Failure($"Пользователь с логином {login} уже существует.");

        var newUser = new User 
        { 
            Login = login, 
            PasswordHash = password, 
            Role = role 
        };
        
        await repository.AddUserAsync(newUser);
        return Result<User>.Success(newUser);
    }
}
