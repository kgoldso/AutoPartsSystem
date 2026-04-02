using AutoPartsSystem.Data;
using AutoPartsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IRepository _repo;
    private readonly IdentityService _identity;
    private readonly WarehouseService _warehouseService;

    public WarehouseController(IRepository repo, IdentityService identity, WarehouseService warehouseService)
    {
        _repo = repo;
        _identity = identity;
        _warehouseService = warehouseService;
    }

    private bool EnsureWarehouse()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role == "Warehouse" || role == "Admin")
        {
            // Simulate login for service layer
            var login = HttpContext.Session.GetString("UserLogin")!;
            var user = _repo.GetUserByLogin(login);
            if (user != null) _identity.Login(login, user.PasswordHash);
            return true;
        }
        return false;
    }

    [HttpGet("pending")]
    public IActionResult GetPending()
    {
        if (!EnsureWarehouse()) return Forbid();

        try
        {
            var orders = _warehouseService.GetPendingShipments();
            var enriched = orders.Select(o =>
            {
                var part = _repo.GetPartById(o.PartId);
                var customer = _repo.GetAllCustomers().FirstOrDefault(c => c.Id == o.CustomerId);
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
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("shipment/{id}")]
    public IActionResult RegisterShipment(int id)
    {
        if (!EnsureWarehouse()) return Forbid();

        try
        {
            _warehouseService.RegisterShipment(id);
            return Ok(new { message = "Отгрузка зарегистрирована" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("inventory")]
    public IActionResult UpdateInventory([FromBody] InventoryRequest request)
    {
        if (!EnsureWarehouse()) return Forbid();

        try
        {
            _warehouseService.UpdateInventory(request.PartId, request.Quantity);
            return Ok(new { message = "Остаток обновлён" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class InventoryRequest
{
    public int PartId { get; set; }
    public int Quantity { get; set; }
}
