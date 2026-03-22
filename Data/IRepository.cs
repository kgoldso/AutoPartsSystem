// Data/IRepository.cs
using AutoPartsSystem.Models;

namespace AutoPartsSystem.Data;

/// <summary>
/// Контракт доступа к данным. Реализации: SqliteRepository (этап 1–3), PostgresRepository (этап 4+).
/// Сервисы зависят только от этого интерфейса — замена БД не затрагивает бизнес-логику.
/// </summary>
public interface IRepository
{
    /// <summary>Добавляет запчасть в каталог. После вставки заполняет Part.Id.</summary>
    void AddPart(Part part);

    List<Part> GetAllParts();
    Part? GetPartById(int id);

    /// <summary>Обновляет остаток на складе. Вызывается при оформлении и отмене заказа.</summary>
    void UpdatePartStock(int partId, int newStock);

    /// <summary>Добавляет клиента. После вставки заполняет Customer.Id.</summary>
    void AddCustomer(Customer customer);

    /// <summary>Ищет клиента по email. Возвращает null если не найден — тогда создаётся новый.</summary>
    Customer? FindCustomerByEmail(string email);

    /// <summary>Добавляет заказ. После вставки заполняет Order.Id.</summary>
    void AddOrder(Order order);

    List<Order> GetAllOrders();
    Order? GetOrderById(int id);

    /// <summary>Меняет статус заказа. Допустимые переходы проверяются в OrderService, не здесь.</summary>
    void UpdateOrderStatus(int orderId, string newStatus);

    /// <summary>Добавляет нового пользователя системы.</summary>
    void AddUser(User user);

    /// <summary>Ищет пользователя по логину для авторизации.</summary>
    User? GetUserByLogin(string login);
}
