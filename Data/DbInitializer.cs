// Data/DbInitializer.cs
using AutoPartsSystem.Models;
using Microsoft.Data.Sqlite;

namespace AutoPartsSystem.Data;

/// <summary>
/// Создаёт схему БД и заполняет тестовые данные при первом запуске.
/// Повторный вызов безопасен — seed не дублирует записи.
/// </summary>
public static class DbInitializer
{
    public static void Initialize(string connectionString)
    {
        using var conn = new SqliteConnection(connectionString);
        conn.Open();
        CreateTables(conn);
        SeedParts(conn);
        SeedUsers(conn);
        SeedCustomersAndOrders(conn);
    }

    private static void CreateTables(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Parts (
                Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                Article      TEXT    NOT NULL UNIQUE,
                Name         TEXT    NOT NULL,
                CarBrand     TEXT    NOT NULL,
                CarModel     TEXT    NOT NULL,
                GroupName    TEXT    NOT NULL,
                Manufacturer TEXT    NOT NULL,
                Price        REAL    NOT NULL,
                Stock        INTEGER NOT NULL,
                LeadTimeDays INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Customers (
                Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                Phone    TEXT NOT NULL,
                Email    TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS Users (
                Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                Login        TEXT    NOT NULL UNIQUE,
                PasswordHash TEXT    NOT NULL,
                Role         TEXT    NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Orders (
                Id                    INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId                INTEGER NOT NULL REFERENCES Users(Id),
                CustomerId            INTEGER NOT NULL REFERENCES Customers(Id),
                CustomerFullName      TEXT    NOT NULL,
                CustomerEmail         TEXT    NOT NULL,
                CustomerPhone         TEXT    NOT NULL,
                PartId                INTEGER NOT NULL REFERENCES Parts(Id),
                Quantity              INTEGER NOT NULL,
                TotalPrice            REAL    NOT NULL,
                Urgent                INTEGER NOT NULL DEFAULT 0,
                Status                TEXT    NOT NULL DEFAULT 'Новый',
                DeliveryMethod        TEXT    NOT NULL,
                OrderDate             TEXT    NOT NULL,
                EstimatedDeliveryDate TEXT    NOT NULL
            );";
        cmd.ExecuteNonQuery();
    }

    private static void SeedParts(SqliteConnection conn)
    {
        var check = conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Parts";
        if ((long)check.ExecuteScalar()! > 0) return;

        var parts = new[]
        {
            new Part { Article = "ENG-001", Name = "Масляный фильтр",            CarBrand = "Toyota",     CarModel = "Camry",    GroupName = "Двигатель",  Manufacturer = "Bosch",      Price = 12.50m,   Stock = 100, LeadTimeDays = 3  },
            new Part { Article = "SUS-002", Name = "Амортизатор передний",       CarBrand = "BMW",        CarModel = "E46",      GroupName = "Подвеска",   Manufacturer = "Sachs",      Price = 89.00m,   Stock = 25,  LeadTimeDays = 7  },
            new Part { Article = "BRK-003", Name = "Тормозные колодки передние", CarBrand = "Volkswagen", CarModel = "Golf",     GroupName = "Тормоза",    Manufacturer = "Brembo",     Price = 45.00m,   Stock = 60,  LeadTimeDays = 5  },
            new Part { Article = "ELC-004", Name = "Аккумулятор 105Ah",           CarBrand = "Ford",       CarModel = "Focus",    GroupName = "Электрика",  Manufacturer = "Varta",      Price = 120.00m,  Stock = 15,  LeadTimeDays = 2  },
            new Part { Article = "BDY-005", Name = "Крыло переднее левое",       CarBrand = "Audi",       CarModel = "A4",       GroupName = "Кузов",      Manufacturer = "Original",   Price = 230.00m,  Stock = 8,   LeadTimeDays = 14 },
            new Part { Article = "ENG-006", Name = "Свеча зажигания IK20",       CarBrand = "Honda",      CarModel = "Civic",    GroupName = "Двигатель",  Manufacturer = "Denso",      Price = 18.50m,   Stock = 200, LeadTimeDays = 2  },
            new Part { Article = "BDY-007", Name = "Капот двигателя",            CarBrand = "Mercedes",   CarModel = "W204",     GroupName = "Кузов",      Manufacturer = "Original",   Price = 350.00m,  Stock = 5,   LeadTimeDays = 10 },
            new Part { Article = "ELC-008", Name = "Стартер двигателя",          CarBrand = "Toyota",     CarModel = "RAV4",     GroupName = "Электрика",  Manufacturer = "Bosch",      Price = 180.00m,  Stock = 12,  LeadTimeDays = 5  },
            new Part { Article = "ELC-009", Name = "Генератор 14V 120A",         CarBrand = "BMW",        CarModel = "E90",      GroupName = "Электрика",  Manufacturer = "Valeo",      Price = 340.00m,  Stock = 5,   LeadTimeDays = 12 },
            new Part { Article = "ENG-010", Name = "Радиатор охлаждения",        CarBrand = "Volkswagen", CarModel = "Passat",   GroupName = "Двигатель",  Manufacturer = "Nissens",    Price = 150.00m,  Stock = 7,   LeadTimeDays = 14 },
            new Part { Article = "BDY-011", Name = "Фара головного света LED",   CarBrand = "Audi",       CarModel = "A3",       GroupName = "Кузов",      Manufacturer = "Hella",      Price = 450.00m,  Stock = 30,  LeadTimeDays = 4  },
            new Part { Article = "BRK-012", Name = "Тормозной диск вентилируемый",CarBrand = "Ford",       CarModel = "Mondeo",   GroupName = "Тормоза",    Manufacturer = "TRW",        Price = 72.00m,   Stock = 18,  LeadTimeDays = 6  },
        };

        foreach (var part in parts)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Parts (Article, Name, CarBrand, CarModel, GroupName, Manufacturer, Price, Stock, LeadTimeDays)
                VALUES (@Article, @Name, @CarBrand, @CarModel, @GroupName, @Manufacturer, @Price, @Stock, @LeadTimeDays)";
            cmd.Parameters.AddWithValue("@Article",      part.Article);
            cmd.Parameters.AddWithValue("@Name",         part.Name);
            cmd.Parameters.AddWithValue("@CarBrand",     part.CarBrand);
            cmd.Parameters.AddWithValue("@CarModel",     part.CarModel);
            cmd.Parameters.AddWithValue("@GroupName",    part.GroupName);
            cmd.Parameters.AddWithValue("@Manufacturer", part.Manufacturer);
            cmd.Parameters.AddWithValue("@Price",        part.Price);
            cmd.Parameters.AddWithValue("@Stock",        part.Stock);
            cmd.Parameters.AddWithValue("@LeadTimeDays", part.LeadTimeDays);
            cmd.ExecuteNonQuery();
        }
    }

    private static void SeedUsers(SqliteConnection conn)
    {
        var check = conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Users";
        if ((long)check.ExecuteScalar()! > 0) return;

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Users (Login, PasswordHash, Role) VALUES 
            ('admin', 'admin123', 'Admin'),
            ('manager', 'man123', 'Manager'),
            ('warehouse', 'store123', 'Warehouse'),
            ('client', 'client123', 'Client');";
        cmd.ExecuteNonQuery();
    }

    private static void SeedCustomersAndOrders(SqliteConnection conn)
    {
        var check = conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Customers";
        if ((long)check.ExecuteScalar()! > 0) return;

        // Seed customers
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Customers (FullName, Phone, Email) VALUES 
            ('Алексей Петров', '+375291234567', 'client@autoparts.by'),
            ('Мария Иванова', '+375297654321', 'maria@mail.ru'),
            ('Сергей Козлов', '+375331112233', 'sergei.k@gmail.com');";
        cmd.ExecuteNonQuery();

        // Seed demo orders with snapshot customer data and stable UserId
        // For demo: client account has ID 4
        var orderCmd = conn.CreateCommand();
        orderCmd.CommandText = @"
            INSERT INTO Orders (UserId, CustomerId, CustomerFullName, CustomerEmail, CustomerPhone, PartId, Quantity, TotalPrice, Urgent, Status, DeliveryMethod, OrderDate, EstimatedDeliveryDate) VALUES 
            (4, 1, 'Алексей Петров', 'client@autoparts.by', '+375291234567', 1, 4, 50.00, 0, 'Отгружен', 'Доставка', '2026-03-20T10:00:00', '2026-03-23T10:00:00'),
            (4, 1, 'Алексей Петров', 'client@autoparts.by', '+375291234567', 3, 2, 90.00, 0, 'Новый', 'Самовывоз', '2026-03-24T14:30:00', '2026-03-29T14:30:00'),
            (4, 2, 'Мария Иванова', 'maria@mail.ru', '+375297654321', 4, 1, 144.00, 1, 'В обработке', 'Доставка', '2026-03-22T09:00:00', '2026-03-23T09:00:00'),
            (4, 2, 'Мария Иванова', 'maria@mail.ru', '+375297654321', 2, 2, 178.00, 0, 'Отгружен', 'Доставка', '2026-03-18T11:00:00', '2026-03-25T11:00:00'),
            (4, 3, 'Сергей Козлов', 'sergei.k@gmail.com', '+375331112233', 5, 1, 276.00, 1, 'Новый', 'Доставка', '2026-03-25T08:00:00', '2026-04-06T08:00:00'),
            (4, 1, 'Алексей Петров', 'client@autoparts.by', '+375291234567', 6, 8, 148.00, 0, 'Отгружен', 'Самовывоз', '2026-03-15T16:00:00', '2026-03-17T16:00:00'),
            (4, 3, 'Сергей Козлов', 'sergei.k@gmail.com', '+375331112233', 8, 2, 136.00, 0, 'В обработке', 'Доставка', '2026-03-23T12:00:00', '2026-03-28T12:00:00');";
        orderCmd.ExecuteNonQuery();
    }
}
