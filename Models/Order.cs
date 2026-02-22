// Models/Order.cs
namespace AutoPartsSystem.Models;

/// <summary>
/// Заказ клиента на одну запчасть.
/// Допустимые статусы: Новый → В обработке → Отгружен | Отменён.
/// </summary>
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }

    /// <summary>Итоговая сумма с учётом срочности (Price * Quantity * 1.2 если Urgent).</summary>
    public decimal TotalPrice { get; set; }

    /// <summary>Срочная поставка: цена ×1.2, срок поставки −2 дня (минимум 1 день).</summary>
    public bool Urgent { get; set; }

    /// <summary>Текущий статус заказа. Отмена доступна только для статусов Новый и В обработке.</summary>
    public string Status { get; set; } = "Новый";

    /// <summary>Способ получения: Самовывоз или Доставка.</summary>
    public string DeliveryMethod { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
}
