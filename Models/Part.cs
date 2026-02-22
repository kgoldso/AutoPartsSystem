// Models/Part.cs
namespace AutoPartsSystem.Models;

/// <summary>
/// Автозапчасть из каталога поставщика.
/// </summary>
public class Part
{
    public int Id { get; set; }

    /// <summary>Уникальный артикул запчасти.</summary>
    public string Article { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string CarBrand { get; set; } = string.Empty;
    public string CarModel { get; set; } = string.Empty;

    /// <summary>Группа: Двигатель, Подвеска, Кузов, Электрика, Тормоза.</summary>
    public string GroupName { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;
    public decimal Price { get; set; }

    /// <summary>Текущий остаток на складе. Уменьшается при заказе, восстанавливается при отмене.</summary>
    public int Stock { get; set; }

    /// <summary>Стандартный срок поставки в днях. При срочном заказе уменьшается на 2, минимум 1.</summary>
    public int LeadTimeDays { get; set; }
}
