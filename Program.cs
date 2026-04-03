using AutoPartsSystem.Data;
using AutoPartsSystem.Models;
using AutoPartsSystem.Services;

const string ConnectionString = "Data Source=autoparts.db";

// 1. Инициализация базы данных
DbInitializer.Initialize(ConnectionString);

// 2. Инициализация слоя данных
var repo = new SqliteRepository(ConnectionString);

// 3. Инициализация сервисного слоя
var identityService = new IdentityService(repo);
var orderService = new OrderService(repo);
var warehouseService = new WarehouseService(repo, identityService);
var adminService = new AdminService(repo, identityService);

Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║              AutoParts Order System — Этап 2                    ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");

// --- ПРИМЕР ИСПОЛЬЗОВАНИЯ ORDER SERVICE ---
Console.WriteLine("\n[Тест] Оформление заказа через OrderService:");

// Вызываем бизнес-логику оформления заказа асинхронно
var orderResult = await orderService.PlaceOrderAsync(
    "customer@email.com", 
    "Алексей Петров", 
    "+375291234567", 
    1, 
    2, 
    isUrgent: true
);

if (orderResult.IsSuccess)
{
    var myOrder = orderResult.Value!;
    Console.WriteLine($"✅ Заказ №{myOrder.Id} успешно оформлен!");
    Console.WriteLine($"   Итоговая цена (с учетом срочности): {myOrder.TotalPrice:F2}");
    Console.WriteLine($"   Ожидаемая дата доставки: {myOrder.EstimatedDeliveryDate:dd.MM.yyyy}");
}
else
{
    Console.WriteLine($"❌ Ошибка оформления: {orderResult.Error}");
}

// --- ПРИМЕР ИСПОЛЬЗОВАНИЯ ADMIN SERVICE ---
// Имитируем логин админа для доступа к отчету
await identityService.LoginAsync("admin", "admin123"); 

Console.WriteLine("\n[Тест] Отчет по остаткам (критический порог < 10):");
var stockReportResult = await adminService.GenerateStockReportAsync(10);

if (stockReportResult.IsSuccess)
{
    foreach (var p in stockReportResult.Value!)
    {
        Console.WriteLine($"⚠️ Мало на складе: {p.Name} (Осталось: {p.Stock})");
    }
}
else
{
    Console.WriteLine($"❌ Ошибка получения отчета: {stockReportResult.Error}");
}

// --- ПРИМЕР ВЫВОДА КАТАЛОГА ---
var parts = await repo.GetAllPartsAsync();
Console.WriteLine("\nТекущее состояние склада:");
Console.WriteLine($"{"ID",-4} {"Артикул",-10} {"Наименование",-32} {"Цена",8} {"Склад",6}");
Console.WriteLine(new string('─', 70));

foreach (var p in parts)
{
    Console.WriteLine($"{p.Id,-4} {p.Article,-10} {p.Name,-32} {p.Price,8:F2} {p.Stock,6}");
}
