using AutoPartsSystem.Data;
using AutoPartsSystem.Services; // Не забудьте добавить этот using

const string ConnectionString = "Data Source=autoparts.db";

// 1. Инициализация базы данных (остается прежней)
DbInitializer.Initialize(ConnectionString);

// 2. Инициализация слоя данных (репозиторий)
var repo = new SqliteRepository(ConnectionString);

// 3. Инициализация сервисного слоя (новое)
// Передаем репозиторий в конструкторы сервисов
var orderService = new OrderService(repo);
var warehouseService = new WarehouseService(repo);
var adminService = new AdminService(repo);

Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║              AutoParts Order System — Этап 2                    ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");

// --- ПРИМЕР ИСПОЛЬЗОВАНИЯ ORDER SERVICE ---
try 
{
    Console.WriteLine("\n[Тест] Оформление заказа через OrderService:");
    
    // Вызываем бизнес-логику оформления заказа
    // Данные: Email, Имя, Телефон, ID детали (например, 1), Кол-во, Срочность
    var myOrder = orderService.PlaceOrder(
        "customer@email.com", 
        "Алексей Петров", 
        "+375291234567", 
        1, 
        2, 
        isUrgent: true
    );

    Console.WriteLine($"✅ Заказ №{myOrder.Id} успешно оформлен!");
    Console.WriteLine($"   Итоговая цена (с учетом срочности): {myOrder.TotalPrice:F2}");
    Console.WriteLine($"   Ожидаемая дата доставки: {myOrder.EstimatedDeliveryDate:dd.MM.yyyy}");
}
catch (Exception ex)
{
    // Сервис сам выбросит исключение, если товара нет на складе
    Console.WriteLine($"❌ Ошибка оформления: {ex.Message}");
}

// --- ПРИМЕР ИСПОЛЬЗОВАНИЯ ADMIN SERVICE ---
Console.WriteLine("\n[Тест] Отчет по остаткам (критический порог < 10):");
var lowStockParts = adminService.GenerateStockReport(10);

foreach (var p in lowStockParts)
{
    Console.WriteLine($"⚠️ Мало на складе: {p.Name} (Осталось: {p.Stock})");
}

// --- ПРИМЕР ВЫВОДА КАТАЛОГА (ваш старый код) ---
var parts = repo.GetAllParts();
Console.WriteLine("\nТекущее состояние склада:");
Console.WriteLine($"{"ID",-4} {"Артикул",-10} {"Наименование",-32} {"Цена",8} {"Склад",6}");
Console.WriteLine(new string('─', 70));

foreach (var p in parts)
{
    Console.WriteLine($"{p.Id,-4} {p.Article,-10} {p.Name,-32} {p.Price,8:F2} {p.Stock,6}");
}