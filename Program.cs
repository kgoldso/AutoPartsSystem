// Program.cs
using AutoPartsSystem.Data;

const string ConnectionString = "Data Source=autoparts.db";

DbInitializer.Initialize(ConnectionString);

var repo = new SqliteRepository(ConnectionString);
var parts = repo.GetAllParts();

Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║              AutoParts Order System — Этап 1                    ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"{"ID",-4} {"Артикул",-10} {"Наименование",-32} {"Марка",-12} {"Модель",-8} {"Группа",-12} {"Произв.",-10} {"Цена",8} {"Склад",6} {"Дней",5}");
Console.WriteLine(new string('─', 115));

foreach (var p in parts)
{
    Console.WriteLine(
        $"{p.Id,-4} {p.Article,-10} {p.Name,-32} {p.CarBrand,-12} {p.CarModel,-8} {p.GroupName,-12} {p.Manufacturer,-10} {p.Price,8:F2} {p.Stock,6} {p.LeadTimeDays,5}");
}

Console.WriteLine(new string('─', 115));
Console.WriteLine($"Итого запчастей в БД: {parts.Count}");
