// PROGRESS.md
# AutoParts Order System — Прогресс разработки

## Стек
- Язык: C# (.NET 8)
- БД (сейчас): SQLite (Microsoft.Data.Sqlite)
- БД (финал): PostgreSQL (Railway)
- UI (сейчас): Консоль
- UI (финал): WinForms → ASP.NET Core + Razor Pages

---

## Этап 1 — Фундамент ✅ ВЫПОЛНЕН
**Цель:** модели, БД, вывод в консоль

- [x] Git-репозиторий, .gitignore
- [x] Модели: `Part`, `Customer`, `Order`
- [x] Интерфейс `IRepository` (Part, Customer, Order)
- [x] `SqliteRepository` — реализация через параметризованный SQL
- [x] `DbInitializer` — создание таблиц + seed 5 запчастей
- [x] `Program.cs` — запуск, инициализация БД, вывод каталога в консоль

**Результат:** проект запускается, создаёт `autoparts.db`, выводит 5 тестовых запчастей.

---

## Этап 2 — Бизнес-логика и консольное меню 🔲 В ПЛАНАХ
**Цель:** вся логика приложения до UI

- [ ] `OrderService.PlaceOrder` — оформление заказа с проверкой склада
- [ ] `OrderService.CancelOrder` — отмена с восстановлением склада
- [ ] `OrderService.ChangeStatus` — смена статуса с валидацией переходов
- [ ] `OrderService.GetOrderHistory` — история заказов по email
- [ ] Консольное меню: каталог, оформление, список заказов, история, смена статуса, отмена

---

## Этап 3 — WinForms UI 🔲 В ПЛАНАХ
**Цель:** графический интерфейс поверх готовой логики (Windows)

- [ ] `MainForm` — таблица заказов, фильтр по статусу
- [ ] `OrderForm` — оформление заказа с живым расчётом суммы и даты
- [ ] `CancelOrderForm` — отмена заказа по номеру и email
- [ ] `CatalogForm` — просмотр и редактирование каталога (опционально)

---

## Этап 4 — Backend API 🔲 В ПЛАНАХ
**Цель:** REST API на ASP.NET Core, PostgreSQL, деплой на Railway

- [ ] `PartsController`, `CustomersController`, `OrdersController`
- [ ] Swagger-документация
- [ ] Переход SQLite → PostgreSQL (`PostgresRepository`)
- [ ] Деплой на Railway через GitHub Actions

---

## Этап 5 — Веб-фронтенд 🔲 В ПЛАНАХ
**Цель:** веб-витрина поверх API

- [ ] Страница каталога с фильтрами
- [ ] Страница оформления заказа
- [ ] Страница «Мои заказы» (по email)
- [ ] Административная панель (опционально)
