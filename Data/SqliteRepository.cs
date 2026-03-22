// Data/SqliteRepository.cs
using AutoPartsSystem.Models;
using Microsoft.Data.Sqlite;

namespace AutoPartsSystem.Data;

/// <summary>
/// Реализация IRepository для SQLite через Microsoft.Data.Sqlite.
/// Все запросы параметризованы — конкатенация строк не используется.
/// </summary>
public class SqliteRepository : IRepository
{
    private readonly string _connectionString;

    public SqliteRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>Открывает и возвращает соединение. Вызывающий код обязан его dispose.</summary>
    private SqliteConnection OpenConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    public void AddPart(Part part)
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Parts (Article, Name, CarBrand, CarModel, GroupName, Manufacturer, Price, Stock, LeadTimeDays)
            VALUES (@Article, @Name, @CarBrand, @CarModel, @GroupName, @Manufacturer, @Price, @Stock, @LeadTimeDays)";
        cmd.Parameters.AddWithValue("@Article", part.Article);
        cmd.Parameters.AddWithValue("@Name", part.Name);
        cmd.Parameters.AddWithValue("@CarBrand", part.CarBrand);
        cmd.Parameters.AddWithValue("@CarModel", part.CarModel);
        cmd.Parameters.AddWithValue("@GroupName", part.GroupName);
        cmd.Parameters.AddWithValue("@Manufacturer", part.Manufacturer);
        cmd.Parameters.AddWithValue("@Price", part.Price);
        cmd.Parameters.AddWithValue("@Stock", part.Stock);
        cmd.Parameters.AddWithValue("@LeadTimeDays", part.LeadTimeDays);
        cmd.ExecuteNonQuery();

        var idCmd = conn.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid()";
        part.Id = (int)(long)idCmd.ExecuteScalar()!;
    }

    public List<Part> GetAllParts()
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Parts ORDER BY Id";
        var parts = new List<Part>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            parts.Add(MapPart(reader));
        return parts;
    }

    public Part? GetPartById(int id)
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Parts WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapPart(reader) : null;
    }

    public void UpdatePartStock(int partId, int newStock)
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Parts SET Stock = @Stock WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Stock", newStock);
        cmd.Parameters.AddWithValue("@Id", partId);
        cmd.ExecuteNonQuery();
    }

    public void AddCustomer(Customer customer)
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Customers (FullName, Phone, Email)
            VALUES (@FullName, @Phone, @Email)";
        cmd.Parameters.AddWithValue("@FullName", customer.FullName);
        cmd.Parameters.AddWithValue("@Phone", customer.Phone);
        cmd.Parameters.AddWithValue("@Email", customer.Email);
        cmd.ExecuteNonQuery();

        var idCmd = conn.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid()";
        customer.Id = (int)(long)idCmd.ExecuteScalar()!;
    }

    public Customer? FindCustomerByEmail(string email)
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Customers WHERE Email = @Email";
        cmd.Parameters.AddWithValue("@Email", email);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapCustomer(reader) : null;
    }

    public void AddOrder(Order order)
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Orders (CustomerId, PartId, Quantity, TotalPrice, Urgent, Status, DeliveryMethod, OrderDate, EstimatedDeliveryDate)
            VALUES (@CustomerId, @PartId, @Quantity, @TotalPrice, @Urgent, @Status, @DeliveryMethod, @OrderDate, @EstimatedDeliveryDate)";
        cmd.Parameters.AddWithValue("@CustomerId", order.CustomerId);
        cmd.Parameters.AddWithValue("@PartId", order.PartId);
        cmd.Parameters.AddWithValue("@Quantity", order.Quantity);
        cmd.Parameters.AddWithValue("@TotalPrice", order.TotalPrice);
        cmd.Parameters.AddWithValue("@Urgent", order.Urgent ? 1 : 0);
        cmd.Parameters.AddWithValue("@Status", order.Status);
        cmd.Parameters.AddWithValue("@DeliveryMethod", order.DeliveryMethod);
        cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate.ToString("o"));
        cmd.Parameters.AddWithValue("@EstimatedDeliveryDate", order.EstimatedDeliveryDate.ToString("o"));
        cmd.ExecuteNonQuery();

        var idCmd = conn.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid()";
        order.Id = (int)(long)idCmd.ExecuteScalar()!;
    }

    public List<Order> GetAllOrders()
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Orders ORDER BY Id";
        var orders = new List<Order>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            orders.Add(MapOrder(reader));
        return orders;
    }

    public Order? GetOrderById(int id)
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Orders WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapOrder(reader) : null;
    }

    public void UpdateOrderStatus(int orderId, string newStatus)
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Orders SET Status = @Status WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Status", newStatus);
        cmd.Parameters.AddWithValue("@Id", orderId);
        cmd.ExecuteNonQuery();
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
        CustomerId            = r.GetInt32(r.GetOrdinal("CustomerId")),
        PartId                = r.GetInt32(r.GetOrdinal("PartId")),
        Quantity              = r.GetInt32(r.GetOrdinal("Quantity")),
        TotalPrice            = (decimal)r.GetDouble(r.GetOrdinal("TotalPrice")),
        Urgent                = r.GetInt32(r.GetOrdinal("Urgent")) == 1,
        Status                = r.GetString(r.GetOrdinal("Status")),
        DeliveryMethod        = r.GetString(r.GetOrdinal("DeliveryMethod")),
        OrderDate             = DateTime.Parse(r.GetString(r.GetOrdinal("OrderDate"))),
        EstimatedDeliveryDate = DateTime.Parse(r.GetString(r.GetOrdinal("EstimatedDeliveryDate")))
    };


    public void AddUser(User user)
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Users (Login, PasswordHash, Role)
            VALUES (@Login, @PasswordHash, @Role)";
        cmd.Parameters.AddWithValue("@Login", user.Login);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash); // В реальности тут должен быть хэш
        cmd.Parameters.AddWithValue("@Role", user.Role);
        cmd.ExecuteNonQuery();

        var idCmd = conn.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid()";
        user.Id = (int)(long)idCmd.ExecuteScalar()!;
    }

    public User? GetUserByLogin(string login)
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Users WHERE Login = @Login";
        cmd.Parameters.AddWithValue("@Login", login);
        using var reader = cmd.ExecuteReader();
        
        if (reader.Read())
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
}
