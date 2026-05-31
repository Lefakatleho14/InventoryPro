# InventoryPro 📦
> A full-stack Inventory Management System built with ASP.NET Core MVC, Entity Framework Core, and Bootstrap 5.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?style=for-the-badge&logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)


What This Project Demonstrates

This project was built as a Final Year **Work Integrated Learning (WIL)** capstone
for a real client — Durban Small Business Supplies. It demonstrates production-ready
full-stack development skills including:

- Secure authentication using cookie-based auth with BCrypt password hashing
- Role-based access control with policy-based authorization
- Full CRUD operations with Entity Framework Core and SQL Server
- PDF & CSV report generation using iTextSharp and CsvHelper
- Real-time UI feedback with AJAX product lookups and live price calculation
- Database design in 3NF with EF Core migrations and automated seeding
- Responsive UI built with Bootstrap 5 and Bootstrap Icons


Live Demo Credentials

| Role | Username | Password |
|------|----------|----------|
| Manager | `lefa_m` | `Manager@123` |
| Shop Assistant | `kago_k` | `Shop123` |


Tech Stack

| Layer | Technology |
|---|---|
| Language | C# (.NET 8.0) |
| Framework | ASP.NET Core MVC |
| Database | SQL Server (via Entity Framework Core 8) |
| ORM | Entity Framework Core + Code-First Migrations |
| Frontend | Bootstrap 5.3, Bootstrap Icons, Vanilla JS |
| Auth | Cookie Authentication + BCrypt.Net |
| PDF Export | iTextSharp 5.5.13 |
| CSV Export | CsvHelper 33 |
| IDE | Visual Studio 2022 |


Screenshots

Login Page
> Secure login with role-based redirection and BCrypt password verification.

Dashboard
> Real-time inventory valuation, today's sales totals, low stock alerts and popup notifications.

Product Management
> Full CRUD with search, filter by stock status, and inline low stock indicators.

Sales Recording
> AJAX-powered product lookup, live total calculation, stock validation, and printable receipt generation.

Reports
> Date-filtered sales reports, inventory valuation, and low stock reports — all exportable to PDF and CSV.


Key Technical Highlights

Security
- Passwords hashed with BCrypt (never stored as plain text)
- Anti-forgery tokens on all POST forms
- Cookie-based authentication with configurable expiry
- Policy-based authorization — Manager vs Shop Assistant roles enforced at controller level
- Unauthorized access redirects to a custom Access Denied page

Database
- 3NF normalized schema with 3 core tables: Users, Products, Sales
- Check constraints enforced at DB level (e.g. price > 0, quantity >= 0)
- Cascade delete prevention — products with sales history cannot be deleted
- Identity columns seeded from custom starting values (Products from 101, Sales from 1001)
- Automated migrations — database created and seeded on first run via `context.Database.Migrate()`

Reporting Engine
- Custom `ReportService` generates styled **PDF reports** using iTextSharp
- **CSV exports** via CsvHelper for spreadsheet compatibility
- Reports include: Sales by date range, Inventory Valuation, Low Stock with recommended order quantities

UX Features
- AJAX product lookup on the sales form — no page reload needed
- Live total calculation as quantity is typed
- Stock validation disables submit button if quantity exceeds available stock
- Low stock modal popup on dashboard load
- Auto-dismissing success alerts after 5 seconds
- Printable receipts with print-specific CSS


Project Structure
InventoryPro/
├── Controllers/
│   ├── AccountController.cs      # Login, Logout, Change Password
│   ├── DashboardController.cs    # Dashboard stats and alerts
│   ├── ProductController.cs      # Product CRUD (Manager only for CUD)
│   ├── SalesController.cs        # Sales recording, receipt, history
│   └── ReportController.cs       # Sales, Valuation, Low Stock reports
├── Models/
│   ├── User.cs                   # System user model
│   ├── Product.cs                # Product model with computed properties
│   └── Sale.cs                   # Sale transaction model
├── ViewModels/
│   ├── LoginViewModel.cs
│   ├── ChangePasswordViewModel.cs
│   └── SalesReportViewModel.cs
├── Data/
│   └── ApplicationDbContext.cs   # EF Core context + seed data
├── Services/
│   └── ReportService.cs          # PDF and CSV generation
├── Views/
│   ├── Account/                  # Login, ChangePassword, AccessDenied
│   ├── Dashboard/                # Main dashboard
│   ├── Product/                  # Index, Create, Edit, Details, Delete
│   ├── Sales/                    # Index, Create, Receipt, Details
│   ├── Report/                   # Sales, Valuation, LowStock
│   └── Shared/                   # Layout, Error, ValidationScripts
├── wwwroot/
│   ├── css/site.css              # Custom styles
│   └── js/site.js                # Global JS utilities
├── Migrations/                   # EF Core auto-generated migrations
├── Program.cs                    # App configuration and DI setup
└── appsettings.json              # Connection string and app settings


Getting Started

Prerequisites
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community is free)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

Installation

1. Clone the repository
```bash
git clone https://github.com/YOUR-USERNAME/InventoryPro.git
cd InventoryPro
```

2. Update the connection string

Open `appsettings.json` and update the server name to match your SQL Server instance:
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=InventoryProDB;
Trusted_Connection=True;TrustServerCertificate=True;"
```

Common server name formats:

.\SQLEXPRESS        → SQL Server Express (most common)
localhost            → Full SQL Server
DESKTOP-XXXX\SQLEXPRESS  → Named instance

3. Apply database migrations

Open **Package Manager Console** in Visual Studio and run:
```powershell
Update-Database
```
This creates the database, all tables, and seeds sample data automatically.

4. Run the application
Press Ctrl+F5 in Visual Studio

The app opens in your browser at `https://localhost:XXXX` and redirects to the login page.


User Roles & Permissions

| Feature | Manager | Shop Assistant |
|---|:---:|:---:|
| View Dashboard | ✅ | ✅ |
| View Products | ✅ | ✅ |
| Add / Edit / Delete Products | ✅ | ❌ |
| Record Sales | ✅ | ✅ |
| View Sales History | ✅ | ✅ |
| View Low Stock Alerts | ✅ | ✅ |
| Sales Report + Export | ✅ | ❌ |
| Inventory Valuation Report | ✅ | ❌ |
| Low Stock Report + Export | ✅ | ❌ |
| Change Own Password | ✅ | ✅ |


NuGet Packages

```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="CsvHelper" Version="33.0.1" />
<PackageReference Include="iTextSharp" Version="5.5.13.3" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
```


Database Schema

Users                    Products                  Sales
─────────────────────    ──────────────────────    ──────────────────────
UserID (PK, Identity)    ProductID (PK, from 101)  SaleID (PK, from 1001)
Username (unique)        ProductName               SaleDate
PasswordHash (BCrypt)    Description               ProductID (FK)
Role                     QuantityInStock           QuantitySold
FullName                 UnitPrice                 TotalPrice
CreatedDate              ReorderLevel              UserID (FK)
LastLoginDate            Supplier
LastUpdated


Team

CodeCrafters — Final Year WIL Project
> Built for Durban Small Business Supplies, Durban, South Africa.


License

This project was built for academic purposes as part of a Work Integrated Learning
programme. Feel free to use it as a reference or learning resource.


Built using ASP.NET Core MVC — CodeCrafters
