# ST2-2025-Team-16-RigShop

## Обща концепция

**RigShop** е уеб приложение, изградено с **ASP.NET Core MVC**, което служи като минималистичен онлайн магазин за РС компоненти и периферия. Потребителите могат да разглеждат продукти по категории, да добавят артикули в количка, да правят поръчки и да създават акаунти. Администраторите могат да създават/редактират/изтриват продукти и да преглеждат всички поръчки.

---

## Технологии и архитектура

### Технологичен стек
- **ASP.NET Core MVC 9** – основна уеб рамка (Model–View–Controller).
- **Entity Framework Core** – ORM за достъп до данни.
- **База данни: SQLite** – конфигурирана чрез `appsettings.json` (файл `rigshop.db`).
- **Razor Views, HTML/CSS/JavaScript** – потребителски интерфейс.
- **Sessions** – управление на сесии за „вписване“ на потребителя и количката.

### Аутентикация (сесийно базирана)
- Съхраняване на `UserId`, `UserEmail` и `IsAdmin` в **HttpContext.Session** след вход.

### Design Patterns
- **MVC** – разделение на Models, Views и Controllers.
- **Dependency Injection** – инжектиране на `ApplicationDbContext` в контролери.
- **ViewModels (частично)** – изгледите използват силно типизирани модели и ViewBag/TempData при нужда.
- **Unit of Work** – `DbContext.SaveChangesAsync()` обединява промени в една транзакция.
- **Session-backed Cart** – количката се пази в сесия чрез помощен клас `SessionExtensions` (JSON serialize/deserialize).

---

## Структура на базата данни (Entities)

Базата се моделира в **EF Core** (виж миграциите в `RigShop/Migrations`). Основни таблици/ентитети:

- **Users**  
  Полета: `Id`, `FullName`, `Email`, `Password`, `Address`, `IsAdmin`  
  > Заб.: паролата е **plain text** в текущия проект.

- **Categories**  
  Полета: `Id`, `Name`  
  Съдържа начални записи („PC Parts“, „GPUs“, „CPUs“, „Motherboards“, „Memory (RAM)“, „Storage“, „Peripherals“, „Monitors“).

- **Products**  
  Полета: `Id`, `Name`, `Description`, `Price`, `Stock`, `ImageUrl`, `CategoryId` (FK)  
  Навигации: `Category`, `OrderItems`.

- **Orders**  
  Полета: `Id`, `CreatedAt`, `UserId` (FK)  
  Навигации: `User`, `Items`  
  Изчислимо свойство: `Total`.

- **OrderItems**  
  Полета: `Id`, `OrderId` (FK), `ProductId` (FK), `Quantity`, `UnitPrice` (snapshot на цената при покупка).

Помощни класове (не са таблици):
- **CartItem** – модел за ред в количката, пазен в сесия: `ProductId`, `Name`, `UnitPrice`, `Quantity`, `ImageUrl`, `LineTotal`.

---

## Функционалности в контролерите

### AuthController – регистрация/вход/изход
- **GET** `/Auth/Register` – показва форма за регистрация.  
- **POST** `/Auth/Register` – създава потребител; записва в БД; *не* прави автоматичен вход.
- **GET** `/Auth/Login` – показва форма за вход.
- **POST** `/Auth/Login` – проверява email/парола, сетва Session (`UserId`, `UserEmail`, `IsAdmin`).
- **POST** `/Auth/Logout` – изчиства сесията.

### ProductsController – управление и преглед на продукти
- **GET** `/Products` – списък с продукти, филтър по категория; показва наличност/цена; бутон **Add to Cart** (за потребители).
- **GET** `/Products/Details/{id}` – подробности за продукт.
- **GET** `/Products/Create` – **само админ**; зарежда списък с категории.
- **POST** `/Products/Create` – **само админ**; валидира и създава продукт.
- **GET** `/Products/Edit/{id}` – **само админ**; зарежда за редакция.
- **POST** `/Products/Edit/{id}` – **само админ**; записва промени.
- **GET** `/Products/Delete/{id}` – **само админ**; потвърждение за изтриване.
- **POST** `/Products/DeleteConfirmed/{id}` – **само админ**; изтрива.

### CartController – количка (съхранена в сесия)
- **GET** `/Cart` – показва редовете в количката, обща сума, бутони за промяна на количества/премахване.
- **POST** `/Cart/Add` – добавя продукт по `productId` и количество `qty` (минимум 1).
- **POST** `/Cart/Update` – променя бройката на даден ред.
- **POST** `/Cart/Remove` – премахва продукт от количката.
- **POST** `/Cart/Checkout` – създава **Order** и съответните **OrderItems**, намалява наличността в `Products.Stock`, изчиства количката.

### OrdersController – поръчки
- **GET** `/Orders` – списък с поръчки:  
  - **Потребител** вижда **само своите** поръчки.
  - **Администратор** вижда **всички** поръчки.
- **GET** `/Orders/Details/{id}` – детайли за конкретна поръчка (разрешено за собственика или за администратор).

### HomeController – начални страници
- **GET** `/` – начална страница.
- **GET** `/Home/Privacy`, `/Home/Error` – статични страници.

---

## Стартиране на проекта (локално)

1. **Изисквания**
   - .NET SDK 9 (или съвместима версия).

2. **Клониране и билд**
   ```bash
   git clone <repo-url>
   cd ST2-2025-Team-16-RigShop/RigShop
   dotnet restore
   dotnet build
   ```

3. **База данни**
   - Конфигурирана е в `appsettings.json`:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=rigshop.db"
     }
     ```
   - Ако желаете да пресъздадете БД от миграциите:
     ```bash
     dotnet ef database update
     ```

4. **Пускане**
   ```bash
   dotnet run
   ```
   Отворете **http://localhost:5000** (или адреса в конзолата).

---

## Права и роли

- **Потребител (IsAdmin = false)**  
  Може да се регистрира/влезе, да разглежда продукти, да добавя в количка, да финализира поръчки, да вижда **само своите** поръчки.

- **Администратор (IsAdmin = true)**  
  Допълнително: може да **създава/редактира/изтрива** продукти, да вижда **всички** поръчки.

---

## Структура на програмата

```
RigShop.sln
└── RigShop/
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── CartController.cs
    │   ├── HomeController.cs
    │   ├── OrdersController.cs
    │   └── ProductsController.cs
    ├── Helpers/SessionExtensions.cs
    ├── Migrations/...
    ├── Models/
    │   ├── CartItem.cs
    │   ├── Category.cs
    │   ├── Order.cs
    │   ├── OrderItem.cs
    │   ├── Product.cs
    │   └── User.cs
    ├── Views/
    │   ├── Auth/...
    │   ├── Cart/...
    │   ├── Home/...
    │   ├── Orders/...
    │   ├── Products/...
    │   └── Shared/_Layout.cshtml (+ .css)
    ├── appsettings.json
    ├── Program.cs
    ├── rigshop.db
    └── wwwroot/...
```

---

