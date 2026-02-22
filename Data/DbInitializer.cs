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

            CREATE TABLE IF NOT EXISTS Orders (
                Id                    INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerId            INTEGER NOT NULL REFERENCES Customers(Id),
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

    /// <summary>
    /// Заполняет 5 тестовых запчастей: по одной на каждую группу (Двигатель, Подвеска, Тормоза, Электрика, Кузов)
    /// и разные марки авто. Пропускает seed если таблица уже содержит записи.
    /// </summary>
    private static void SeedParts(SqliteConnection conn)
    {
        var check = conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Parts";
        if ((long)check.ExecuteScalar()! > 0) return;

        var parts = new[]
        {
            new Part { Article = "ENG-001", Name = "Масляный фильтр",            CarBrand = "Toyota",     CarModel = "Camry", GroupName = "Двигатель", Manufacturer = "Bosch",    Price = 12.50m,  Stock = 100, LeadTimeDays = 3  },
            new Part { Article = "SUS-002", Name = "Амортизатор передний",       CarBrand = "BMW",        CarModel = "E46",   GroupName = "Подвеска",  Manufacturer = "Sachs",    Price = 89.00m,  Stock = 25,  LeadTimeDays = 7  },
            new Part { Article = "BRK-003", Name = "Тормозные колодки передние", CarBrand = "Volkswagen", CarModel = "Golf",  GroupName = "Тормоза",   Manufacturer = "Brembo",   Price = 45.00m,  Stock = 60,  LeadTimeDays = 5  },
            new Part { Article = "ELC-004", Name = "Аккумулятор 60Ah",           CarBrand = "Ford",       CarModel = "Focus", GroupName = "Электрика", Manufacturer = "Varta",    Price = 120.00m, Stock = 15,  LeadTimeDays = 2  },
            new Part { Article = "BDY-005", Name = "Крыло переднее левое",       CarBrand = "Audi",       CarModel = "A4",    GroupName = "Кузов",     Manufacturer = "Original", Price = 230.00m, Stock = 8,   LeadTimeDays = 14 }
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
}
