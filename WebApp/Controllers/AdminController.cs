using AutoPartsSystem.Data;
using AutoPartsSystem.Models;
using AutoPartsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IRepository _repo;
    private readonly IdentityService _identity;
    private readonly AdminService _adminService;

    public AdminController(IRepository repo, IdentityService identity, AdminService adminService)
    {
        _repo = repo;
        _identity = identity;
        _adminService = adminService;
    }

    private bool EnsureAdmin()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role == "Admin")
        {
            var login = HttpContext.Session.GetString("UserLogin")!;
            var user = _repo.GetUserByLogin(login);
            if (user != null) _identity.Login(login, user.PasswordHash);
            return true;
        }
        return false;
    }

    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        if (!EnsureAdmin()) return Forbid();

        var orders = _repo.GetAllOrders();
        var parts = _repo.GetAllParts();
        var customers = _repo.GetAllCustomers();
        var users = _repo.GetAllUsers();

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
    public IActionResult SalesReport()
    {
        if (!EnsureAdmin()) return Forbid();

        try
        {
            var total = _adminService.GenerateSalesReport();
            var orders = _repo.GetAllOrders()
                .Where(o => o.Status == "Отгружен")
                .OrderByDescending(o => o.OrderDate)
                .Select(o =>
                {
                    var part = _repo.GetPartById(o.PartId);
                    return new { o.Id, o.TotalPrice, o.OrderDate, PartName = part?.Name ?? "—" };
                }).ToList();

            return Ok(new { totalRevenue = total, orders });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("reports/stock")]
    public IActionResult StockReport()
    {
        if (!EnsureAdmin()) return Forbid();

        try
        {
            var lowStock = _adminService.GenerateStockReport(10);
            return Ok(lowStock);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        if (!EnsureAdmin()) return Forbid();
        var users = _repo.GetAllUsers().Select(u => new { u.Id, u.Login, u.Role }).ToList();
        return Ok(users);
    }

    [HttpPost("users")]
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        if (!EnsureAdmin()) return Forbid();

        try
        {
            var user = _adminService.RegisterEmployee(request.Login, request.Password, request.Role);
            return Ok(new { message = "Пользователь создан", id = user.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("users/{id}")]
    public IActionResult DeleteUser(int id)
    {
        if (!EnsureAdmin()) return Forbid();
        _repo.DeleteUser(id);
        return Ok(new { message = "Пользователь удалён" });
    }

    [HttpPost("parts")]
    public IActionResult AddPart([FromBody] Part part)
    {
        if (!EnsureAdmin()) return Forbid();
        _repo.AddPart(part);
        return Ok(new { message = "Запчасть добавлена", id = part.Id });
    }

    [HttpPut("parts/{id}")]
    public IActionResult UpdatePart(int id, [FromBody] Part part)
    {
        if (!EnsureAdmin()) return Forbid();
        part.Id = id;
        _repo.UpdatePart(part);
        return Ok(new { message = "Запчасть обновлена" });
    }

    [HttpDelete("parts/{id}")]
    public IActionResult DeletePart(int id)
    {
        if (!EnsureAdmin()) return Forbid();
        _repo.DeletePart(id);
        return Ok(new { message = "Запчасть удалена" });
    }
}

public class CreateUserRequest
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
}
