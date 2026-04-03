namespace AutoPartsSystem.Models;

/// <summary>
/// Заказ клиента на одну запчасть.
/// Допустимые статусы: Новый → В обработке → Отгружен | Отменён.
/// </summary>
public class Order
{
    public int Id { get; set; }
    
    /// <summary>ID пользователя (аккаунта), совершившего заказ.</summary>
    public int UserId { get; set; }
    
    public int CustomerId { get; set; }
    
    // Snapshot of customer data at the time of order
    public required string CustomerFullName { get; set; }
    public required string CustomerEmail { get; set; }
    public required string CustomerPhone { get; set; }

    public int PartId { get; set; }
    public int Quantity { get; set; }

    /// <summary>Итоговая сумма с учётом срочности.</summary>
    public decimal TotalPrice { get; set; }

    /// <summary>Срочная поставка.</summary>
    public bool Urgent { get; set; }

    /// <summary>Текущий статус заказа.</summary>
    public required string Status { get; set; }

    /// <summary>Способ получения.</summary>
    public required string DeliveryMethod { get; set; }

    public DateTime OrderDate { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
}
