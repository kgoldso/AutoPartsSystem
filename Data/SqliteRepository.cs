using AutoPartsSystem.Models;
using Microsoft.Data.Sqlite;

namespace AutoPartsSystem.Data;

/// <summary>
/// Реализация IRepository для SQLite через Microsoft.Data.Sqlite.
/// Использует асинхронный доступ и первичные конструкторы (C# 12).
/// </summary>
public class SqliteRepository(string connectionString) : IRepository
{
    private async Task<SqliteConnection> OpenConnectionAsync()
    {
        var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync();
        return conn;
    }

    public async Task AddPartAsync(Part part)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Parts (Article, Name, CarBrand, CarModel, GroupName, Manufacturer, Price, Stock, LeadTimeDays)
            VALUES (@Article, @Name, @CarBrand, @CarModel, @GroupName, @Manufacturer, @Price, @Stock, @LeadTimeDays)
            """;
        cmd.Parameters.AddWithValue("@Article", part.Article);
        cmd.Parameters.AddWithValue("@Name", part.Name);
        cmd.Parameters.AddWithValue("@CarBrand", part.CarBrand);
        cmd.Parameters.AddWithValue("@CarModel", part.CarModel);
        cmd.Parameters.AddWithValue("@GroupName", part.GroupName);
        cmd.Parameters.AddWithValue("@Manufacturer", part.Manufacturer);
        cmd.Parameters.AddWithValue("@Price", part.Price);
        cmd.Parameters.AddWithValue("@Stock", part.Stock);
        cmd.Parameters.AddWithValue("@LeadTimeDays", part.LeadTimeDays);
        await cmd.ExecuteNonQueryAsync();

        var idCmd = conn.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid()";
        part.Id = (int)(long)(await idCmd.ExecuteScalarAsync() ?? 0L);
    }

    public async Task<List<Part>> GetAllPartsAsync()
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Parts ORDER BY Id";
        List<Part> parts = [];
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            parts.Add(MapPart(reader));
        return parts;
    }

    public async Task<Part?> GetPartByIdAsync(int id)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Parts WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapPart(reader) : null;
    }

    public async Task UpdatePartStockAsync(int partId, int newStock)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Parts SET Stock = @Stock WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Stock", newStock);
        cmd.Parameters.AddWithValue("@Id", partId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task AddCustomerAsync(Customer customer)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Customers (FullName, Phone, Email)
            VALUES (@FullName, @Phone, @Email)
            """;
        cmd.Parameters.AddWithValue("@FullName", customer.FullName);
        cmd.Parameters.AddWithValue("@Phone", customer.Phone);
        cmd.Parameters.AddWithValue("@Email", customer.Email);
        await cmd.ExecuteNonQueryAsync();

        var idCmd = conn.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid()";
        customer.Id = (int)(long)(await idCmd.ExecuteScalarAsync() ?? 0L);
    }

    public async Task<Customer?> FindCustomerByEmailAsync(string email)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Customers WHERE Email = @Email";
        cmd.Parameters.AddWithValue("@Email", email);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapCustomer(reader) : null;
    }

    public async Task AddOrderAsync(Order order)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Orders (UserId, CustomerId, CustomerFullName, CustomerEmail, CustomerPhone, PartId, Quantity, TotalPrice, Urgent, Status, DeliveryMethod, OrderDate, EstimatedDeliveryDate)
            VALUES (@UserId, @CustomerId, @CustomerFullName, @CustomerEmail, @CustomerPhone, @PartId, @Quantity, @TotalPrice, @Urgent, @Status, @DeliveryMethod, @OrderDate, @EstimatedDeliveryDate)
            """;
        cmd.Parameters.AddWithValue("@UserId", order.UserId);
        cmd.Parameters.AddWithValue("@CustomerId", order.CustomerId);
        cmd.Parameters.AddWithValue("@CustomerFullName", order.CustomerFullName);
        cmd.Parameters.AddWithValue("@CustomerEmail", order.CustomerEmail);
        cmd.Parameters.AddWithValue("@CustomerPhone", order.CustomerPhone);
        cmd.Parameters.AddWithValue("@PartId", order.PartId);
        cmd.Parameters.AddWithValue("@Quantity", order.Quantity);
        cmd.Parameters.AddWithValue("@TotalPrice", order.TotalPrice);
        cmd.Parameters.AddWithValue("@Urgent", order.Urgent ? 1 : 0);
        cmd.Parameters.AddWithValue("@Status", order.Status);
        cmd.Parameters.AddWithValue("@DeliveryMethod", order.DeliveryMethod);
        cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate.ToString("o"));
        cmd.Parameters.AddWithValue("@EstimatedDeliveryDate", order.EstimatedDeliveryDate.ToString("o"));
        await cmd.ExecuteNonQueryAsync();

        var idCmd = conn.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid()";
        order.Id = (int)(long)(await idCmd.ExecuteScalarAsync() ?? 0L);
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Orders ORDER BY Id";
        List<Order> orders = [];
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            orders.Add(MapOrder(reader));
        return orders;
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Orders WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapOrder(reader) : null;
    }

    public async Task UpdateOrderStatusAsync(int orderId, string newStatus)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Orders SET Status = @Status WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Status", newStatus);
        cmd.Parameters.AddWithValue("@Id", orderId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task AddUserAsync(User user)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Users (Login, PasswordHash, Role)
            VALUES (@Login, @PasswordHash, @Role)
            """;
        cmd.Parameters.AddWithValue("@Login", user.Login);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@Role", user.Role);
        await cmd.ExecuteNonQueryAsync();

        var idCmd = conn.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid()";
        user.Id = (int)(long)(await idCmd.ExecuteScalarAsync() ?? 0L);
    }

    public async Task<User?> GetUserByLoginAsync(string login)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Users WHERE Login = @Login";
        cmd.Parameters.AddWithValue("@Login", login);
        using var reader = await cmd.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Login = reader.GetString(reader.GetOrdinal("Login")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role"))
            };
        }
        return null;
    }

    public async Task<List<Part>> SearchPartsAsync(string? query, string? group)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        List<string> conditions = [];

        if (!string.IsNullOrWhiteSpace(group))
        {
            conditions.Add("GroupName = @Group");
            cmd.Parameters.AddWithValue("@Group", group);
        }

        var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
        cmd.CommandText = $"SELECT * FROM Parts {where} ORDER BY Id";

        List<Part> parts = [];
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            parts.Add(MapPart(reader));

        if (!string.IsNullOrWhiteSpace(query))
        {
            parts = parts.Where(p => 
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Article.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.CarBrand.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.CarModel.Contains(query, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        return parts;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Users ORDER BY Id";
        List<User> users = [];
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Login = reader.GetString(reader.GetOrdinal("Login")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role"))
            });
        }
        return users;
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Customers ORDER BY Id";
        List<Customer> customers = [];
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            customers.Add(MapCustomer(reader));
        return customers;
    }

    public async Task DeleteUserAsync(int userId)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Users WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", userId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdatePartAsync(Part part)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            UPDATE Parts SET Article=@Article, Name=@Name, CarBrand=@CarBrand, CarModel=@CarModel,
            GroupName=@GroupName, Manufacturer=@Manufacturer, Price=@Price, Stock=@Stock, LeadTimeDays=@LeadTimeDays
            WHERE Id=@Id
            """;
        cmd.Parameters.AddWithValue("@Id", part.Id);
        cmd.Parameters.AddWithValue("@Article", part.Article);
        cmd.Parameters.AddWithValue("@Name", part.Name);
        cmd.Parameters.AddWithValue("@CarBrand", part.CarBrand);
        cmd.Parameters.AddWithValue("@CarModel", part.CarModel);
        cmd.Parameters.AddWithValue("@GroupName", part.GroupName);
        cmd.Parameters.AddWithValue("@Manufacturer", part.Manufacturer);
        cmd.Parameters.AddWithValue("@Price", part.Price);
        cmd.Parameters.AddWithValue("@Stock", part.Stock);
        cmd.Parameters.AddWithValue("@LeadTimeDays", part.LeadTimeDays);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeletePartAsync(int partId)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Parts WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", partId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        using var conn = await OpenConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Customers SET FullName = @FullName, Phone = @Phone WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@FullName", customer.FullName);
        cmd.Parameters.AddWithValue("@Phone", customer.Phone);
        cmd.Parameters.AddWithValue("@Id", customer.Id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static Part MapPart(SqliteDataReader r) => new()
    {
        Id           = r.GetInt32(r.GetOrdinal("Id")),
        Article      = r.GetString(r.GetOrdinal("Article")),
        Name         = r.GetString(r.GetOrdinal("Name")),
        CarBrand     = r.GetString(r.GetOrdinal("CarBrand")),
        CarModel     = r.GetString(r.GetOrdinal("CarModel")),
        GroupName    = r.GetString(r.GetOrdinal("GroupName")),
        Manufacturer = r.GetString(r.GetOrdinal("Manufacturer")),
        Price        = (decimal)r.GetDouble(r.GetOrdinal("Price")),
        Stock        = r.GetInt32(r.GetOrdinal("Stock")),
        LeadTimeDays = r.GetInt32(r.GetOrdinal("LeadTimeDays"))
    };

    private static Customer MapCustomer(SqliteDataReader r) => new()
    {
        Id       = r.GetInt32(r.GetOrdinal("Id")),
        FullName = r.GetString(r.GetOrdinal("FullName")),
        Phone    = r.GetString(r.GetOrdinal("Phone")),
        Email    = r.GetString(r.GetOrdinal("Email"))
    };

    private static Order MapOrder(SqliteDataReader r) => new()
    {
        Id                    = r.GetInt32(r.GetOrdinal("Id")),
        UserId                = r.GetInt32(r.GetOrdinal("UserId")),
        CustomerId            = r.GetInt32(r.GetOrdinal("CustomerId")),
        CustomerFullName      = r.GetString(r.GetOrdinal("CustomerFullName")),
        CustomerEmail         = r.GetString(r.GetOrdinal("CustomerEmail")),
        CustomerPhone         = r.GetString(r.GetOrdinal("CustomerPhone")),
        PartId                = r.GetInt32(r.GetOrdinal("PartId")),
        Quantity              = r.GetInt32(r.GetOrdinal("Quantity")),
        TotalPrice            = (decimal)r.GetDouble(r.GetOrdinal("TotalPrice")),
        Urgent                = r.GetInt32(r.GetOrdinal("Urgent")) == 1,
        Status                = r.GetString(r.GetOrdinal("Status")),
        DeliveryMethod        = r.GetString(r.GetOrdinal("DeliveryMethod")),
        OrderDate             = DateTime.Parse(r.GetString(r.GetOrdinal("OrderDate"))),
        EstimatedDeliveryDate = DateTime.Parse(r.GetString(r.GetOrdinal("EstimatedDeliveryDate")))
    };
}
