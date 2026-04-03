namespace AutoPartsSystem.Models;

/// <summary>
/// Клиент поставщика. Идентифицируется по уникальному Email.
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Phone { get; set; }

    /// <summary>Email уникален.</summary>
    public required string Email { get; set; }
}
