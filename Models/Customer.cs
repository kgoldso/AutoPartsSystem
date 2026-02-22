// Models/Customer.cs
namespace AutoPartsSystem.Models;

/// <summary>
/// Клиент поставщика. Идентифицируется по уникальному Email.
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    /// <summary>Email уникален. Используется для поиска существующего клиента при оформлении заказа.</summary>
    public string Email { get; set; } = string.Empty;
}
