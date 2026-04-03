using AutoPartsSystem.Data;
using AutoPartsSystem.Models;
using AutoPartsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IRepository repo, OrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role == null) return Unauthorized();

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var orders = await repo.GetAllOrdersAsync();
        
        // Filter by UserId if Client - this ensures orders are always visible to the account owner
        if (role == "Client")
        {
            orders = orders.Where(o => o.UserId == userId.Value).ToList();
        }

        var parts = await repo.GetAllPartsAsync();

        // Enrich with part names and customer info from order snapshot
        var enriched = orders.Select(o =>
        {
            var part = parts.FirstOrDefault(p => p.Id == o.PartId);
            return new
            {
                o.Id, o.CustomerId, o.PartId, o.Quantity, o.TotalPrice, o.Urgent,
                o.Status, o.DeliveryMethod, o.OrderDate, o.EstimatedDeliveryDate,
                PartName = part?.Name ?? "—",
                PartArticle = part?.Article ?? "—",
                CustomerName = o.CustomerFullName,
                CustomerEmail = o.CustomerEmail
            };
        }).OrderByDescending(o => o.OrderDate).ToList();

        return Ok(enriched);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role == null) return Unauthorized();

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var result = await orderService.PlaceOrderAsync(
            userId.Value,
            request.Email, request.FullName, request.Phone,
            request.PartId, request.Quantity, request.IsUrgent);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        var order = result.Value!;
        return Ok(new { message = "Заказ оформлен", orderId = order.Id, totalPrice = order.TotalPrice });
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Manager" && role != "Admin")
            return Forbid();

        if (request.Status is "Отменен" or "Отменён")
        {
            var cancelResult = await orderService.CancelOrderAsync(id);
            if (!cancelResult.IsSuccess)
                return BadRequest(new { message = cancelResult.Error });
            
            return Ok(new { message = "Заказ отменён, товар возвращён на склад" });
        }

        var order = await repo.GetOrderByIdAsync(id);
        if (order == null) return NotFound();

        await repo.UpdateOrderStatusAsync(id, request.Status);
        return Ok(new { message = "Статус обновлён" });
    }
}

public class PlaceOrderRequest
{
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public bool IsUrgent { get; set; }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = "";
}
