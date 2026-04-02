using AutoPartsSystem.Data;
using AutoPartsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IRepository _repo;
    private readonly OrderService _orderService;

    public OrdersController(IRepository repo, OrderService orderService)
    {
        _repo = repo;
        _orderService = orderService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role == null) return Unauthorized();

        var orders = _repo.GetAllOrders();

        // Enrich with part names and customer info
        var enriched = orders.Select(o =>
        {
            var part = _repo.GetPartById(o.PartId);
            var customer = _repo.GetAllCustomers().FirstOrDefault(c => c.Id == o.CustomerId);
            return new
            {
                o.Id, o.CustomerId, o.PartId, o.Quantity, o.TotalPrice, o.Urgent,
                o.Status, o.DeliveryMethod, o.OrderDate, o.EstimatedDeliveryDate,
                PartName = part?.Name ?? "—",
                PartArticle = part?.Article ?? "—",
                CustomerName = customer?.FullName ?? "—",
                CustomerEmail = customer?.Email ?? "—"
            };
        }).OrderByDescending(o => o.OrderDate).ToList();

        return Ok(enriched);
    }

    [HttpPost]
    public IActionResult PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role == null) return Unauthorized();

        try
        {
            var order = _orderService.PlaceOrder(
                request.Email, request.FullName, request.Phone,
                request.PartId, request.Quantity, request.IsUrgent);

            return Ok(new { message = "Заказ оформлен", orderId = order.Id, totalPrice = order.TotalPrice });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Manager" && role != "Admin")
            return Forbid();

        var order = _repo.GetOrderById(id);
        if (order == null) return NotFound();

        _repo.UpdateOrderStatus(id, request.Status);
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
