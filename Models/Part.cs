namespace AutoPartsSystem.Models;

/// <summary>
/// Автозапчасть из каталога поставщика.
/// </summary>
public class Part
{
    public int Id { get; set; }

    /// <summary>Уникальный артикул запчасти.</summary>
    public required string Article { get; set; }

    public required string Name { get; set; }
    public required string CarBrand { get; set; }
    public required string CarModel { get; set; }

    /// <summary>Группа: Двигатель, Подвеска, Кузов, Электрика, Тормоза.</summary>
    public required string GroupName { get; set; }

    public required string Manufacturer { get; set; }
    public decimal Price { get; set; }

    /// <summary>Текущий остаток на складе.</summary>
    public int Stock { get; set; }

    /// <summary>Стандартный срок поставки в днях.</summary>
    public int LeadTimeDays { get; set; }
}
