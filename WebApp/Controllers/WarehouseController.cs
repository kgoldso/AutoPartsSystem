using AutoPartsSystem.Data;
using AutoPartsSystem.Models;
using AutoPartsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController(IRepository repo, IdentityService identity, WarehouseService warehouseService) : ControllerBase
{
    private async Task<Result> EnsureWarehouseAsync()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Warehouse" && role != "Admin") return Result.Failure("Forbidden");

        var login = HttpContext.Session.GetString("UserLogin");
        if (login == null) return Result.Failure("Unauthorized");

        var user = await repo.GetUserByLoginAsync(login);
        if (user == null) return Result.Failure("Unauthorized");

        await identity.LoginAsync(login, user.PasswordHash);
        return Result.Success();
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var auth = await EnsureWarehouseAsync();
        if (!auth.IsSuccess) return auth.Error == "Forbidden" ? Forbid() : Unauthorized();

        var result = await warehouseService.GetPendingShipmentsAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.Error });

        var orders = result.Value!;
        var parts = await repo.GetAllPartsAsync();
        var customers = await repo.GetAllCustomersAsync();

        var enriched = orders.Select(o =>
        {
            var part = parts.FirstOrDefault(p => p.Id == o.PartId);
            var customer = customers.FirstOrDefault(c => c.Id == o.CustomerId);
            return new
            {
                o.Id, o.CustomerId, o.PartId, o.Quantity, o.TotalPrice,
                o.Urgent, o.Status, o.OrderDate,
                PartName = part?.Name ?? "—",
                PartArticle = part?.Article ?? "—",
                CustomerName = customer?.FullName ?? "—"
            };
        }).ToList();

        return Ok(enriched);
    }

    [HttpPost("shipment/{id}")]
    public async Task<IActionResult> RegisterShipment(int id)
    {
        var auth = await EnsureWarehouseAsync();
        if (!auth.IsSuccess) return auth.Error == "Forbidden" ? Forbid() : Unauthorized();

        var result = await warehouseService.RegisterShipmentAsync(id);
        if (!result.IsSuccess) return BadRequest(new { message = result.Error });

        return Ok(new { message = "Отгрузка зарегистрирована" });
    }

    [HttpPost("inventory")]
    public async Task<IActionResult> UpdateInventory([FromBody] InventoryRequest request)
    {
        var auth = await EnsureWarehouseAsync();
        if (!auth.IsSuccess) return auth.Error == "Forbidden" ? Forbid() : Unauthorized();

        var result = await warehouseService.UpdateInventoryAsync(request.PartId, request.Quantity);
        if (!result.IsSuccess) return BadRequest(new { message = result.Error });

        return Ok(new { message = "Остаток обновлён" });
    }
}

public class InventoryRequest
{
    public int PartId { get; set; }
    public int Quantity { get; set; }
}
