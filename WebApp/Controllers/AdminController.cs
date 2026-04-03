using AutoPartsSystem.Data;
using AutoPartsSystem.Models;
using AutoPartsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController(IRepository repo, IdentityService identity, AdminService adminService) : ControllerBase
{
    private async Task<Result> EnsureAdminAsync()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Admin") return Result.Failure("Forbidden");

        var login = HttpContext.Session.GetString("UserLogin");
        if (login == null) return Result.Failure("Unauthorized");

        var user = await repo.GetUserByLoginAsync(login);
        if (user == null) return Result.Failure("Unauthorized");

        await identity.LoginAsync(login, user.PasswordHash);
        return Result.Success();
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var auth = await EnsureAdminAsync();
        if (!auth.IsSuccess) return auth.Error == "Forbidden" ? Forbid() : Unauthorized();

        var orders = await repo.GetAllOrdersAsync();
        var parts = await repo.GetAllPartsAsync();
        var customers = await repo.GetAllCustomersAsync();
        var users = await repo.GetAllUsersAsync();

        var totalRevenue = orders.Where(o => o.Status == "Отгружен").Sum(o => o.TotalPrice);
        var totalOrders = orders.Count;
        var pendingOrders = orders.Count(o => o.Status == "Новый" || o.Status == "В обработке");
        var lowStock = parts.Count(p => p.Stock < 10);

        return Ok(new
        {
            totalRevenue,
            totalOrders,
            pendingOrders,
            shippedOrders = orders.Count(o => o.Status == "Отгружен"),
            totalParts = parts.Count,
            lowStock,
            totalCustomers = customers.Count,
            totalUsers = users.Count
        });
    }

    [HttpGet("reports/sales")]
    public async Task<IActionResult> SalesReport()
    {
        var auth = await EnsureAdminAsync();
        if (!auth.IsSuccess) return auth.Error == "Forbidden" ? Forbid() : Unauthorized();

        var result = await adminService.GenerateSalesReportAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.Error });

        var allOrders = await repo.GetAllOrdersAsync();
        var allParts = await repo.GetAllPartsAsync();

        var orders = allOrders
            .Where(o => o.Status == "Отгружен")
            .OrderByDescending(o => o.OrderDate)
            .Select(o =>
            {
                var part = allParts.FirstOrDefault(p => p.Id == o.PartId);
                return new { o.Id, o.TotalPrice, o.OrderDate, PartName = part?.Name ?? "—" };
            }).ToList();

        return Ok(new { totalRevenue = result.Value, orders });
    }

    [HttpGet("reports/stock")]
    public async Task<IActionResult> StockReport()
    {
        var auth = await EnsureAdminAsync();
        if (!auth.IsSuccess) return auth.Error == "Forbidden" ? Forbid() : Unauthorized();

        var result = await adminService.GenerateStockReportAsync(10);
        if (!result.IsSuccess) return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var auth = await EnsureAdminAsync();
        if (!auth.IsSuccess) return auth.Error == "Forbidden" ? Forbid() : Unauthorized();

        var users = await repo.GetAllUsersAsync();
        var projection = users.Select(u => new { u.Id, u.Login, u.Role }).ToList();
        return Ok(projection);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var auth = await EnsureAdminAsync();
        if (!auth.IsSuccess) return auth.Error == "Forbidden" ? Forbid() : Unauthorized();

        var result = await adminService.RegisterEmployeeAsync(request.Login, request.Password, request.Role);
        if (!result.IsSuccess) return BadRequest(new { message = result.Error });

        return Ok(new { message = "Пользователь создан", id = result.Value!.Id });
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var auth = await EnsureAdminAsync();
        if (!auth.IsSuccess) return auth.Error == "Forbidden" ? Forbid() : Unauthorized();

        await repo.DeleteUserAsync(id);
        return Ok(new { message = "Пользователь удалён" });
    }

    [HttpPost("parts")]
    public async Task<IActionResult> AddPart([FromBody] Part part)
    {
        var auth = await EnsureAdminAsync();
        if (!auth.IsSuccess) return auth.Error == "Forbidden" ? Forbid() : Unauthorized();

        await repo.AddPartAsync(part);
        return Ok(new { message = "Запчасть добавлена", id = part.Id });
    }

    [HttpPut("parts/{id}")]
    public async Task<IActionResult> UpdatePart(int id, [FromBody] Part part)
    {
        var auth = await EnsureAdminAsync();
        if (!auth.IsSuccess) return auth.Error == "Forbidden" ? Forbid() : Unauthorized();

        part.Id = id;
        await repo.UpdatePartAsync(part);
        return Ok(new { message = "Запчасть обновлена" });
    }

    [HttpDelete("parts/{id}")]
    public async Task<IActionResult> DeletePart(int id)
    {
        var auth = await EnsureAdminAsync();
        if (!auth.IsSuccess) return auth.Error == "Forbidden" ? Forbid() : Unauthorized();

        await repo.DeletePartAsync(id);
        return Ok(new { message = "Запчасть удалена" });
    }
}

public class CreateUserRequest
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
}
