using AutoPartsSystem.Models;

namespace AutoPartsSystem.Data;

/// <summary>
/// Контракт доступа к данным. Все операции асинхронны.
/// </summary>
public interface IRepository
{
    Task AddPartAsync(Part part);
    Task<List<Part>> GetAllPartsAsync();
    Task<Part?> GetPartByIdAsync(int id);
    Task UpdatePartStockAsync(int partId, int newStock);

    Task AddCustomerAsync(Customer customer);
    Task<Customer?> FindCustomerByEmailAsync(string email);
    Task UpdateCustomerAsync(Customer customer);

    Task AddOrderAsync(Order order);
    Task<List<Order>> GetAllOrdersAsync();
    Task<Order?> GetOrderByIdAsync(int id);
    Task UpdateOrderStatusAsync(int orderId, string newStatus);

    Task AddUserAsync(User user);
    Task<User?> GetUserByLoginAsync(string login);
    Task<List<User>> GetAllUsersAsync();
    Task DeleteUserAsync(int userId);

    Task<List<Part>> SearchPartsAsync(string? query, string? group);
    Task<List<Customer>> GetAllCustomersAsync();
    Task UpdatePartAsync(Part part);
    Task DeletePartAsync(int partId);
}
