# TechShop ECommerce Web App

A full-stack e-commerce application built with ASP.NET Core Web API (.NET 10) and Angular 19. The backend exposes a RESTful API connected to MS SQL Server (LocalDB), and the frontend is a Single Page Application that consumes it.

---

## Prerequisites

Before running the project, ensure you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v18 or later) and npm
- [Angular CLI](https://angular.io/cli) v19: `npm install -g @angular/cli`
- SQL Server LocalDB (included with Visual Studio, or install [SQL Server Express with LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb))

---

## Repository Structure

```
ECommerce/
├── ECommerce.Server/           # .NET backend solution
│   ├── ECommerceAPI/           # ASP.NET Core Web API project
│   └── ECommerceAPI.Tests/     # xUnit unit tests
├── ECommerce.client/           # Angular 19 frontend
├── database-setup-localdb.sql  # SQL script to create and seed the database
├── ECommerceDB.dacpac          # Database backup with populated products
└── README.md
```

---

## 1. Database Setup

You have two options to set up the database.

### Option A: Using the SQL script (recommended for LocalDB)

1. Open SQL Server Object Explorer in Visual Studio (View > SQL Server Object Explorer).
2. Connect to `(localdb)\MSSQLLocalDB`.
3. Right-click the server and select **New Query**.
4. Open the file `database-setup-localdb.sql`, paste its contents into the query window, and execute it.

This creates the `ECommerceDB` database with all tables and seeds 12 products.

### Option B: Restoring the .dacpac file

1. In Visual Studio, open SQL Server Object Explorer.
2. Right-click on **Databases** under `(localdb)\MSSQLLocalDB`.
3. Select **Publish Data-tier Application** and follow the wizard, pointing to `ECommerceDB.dacpac`.

---

## 2. Running the Backend (.NET API)

The API uses the connection string below (already configured in `appsettings.json`):

```
Server=(localdb)\MSSQLLocalDB;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;
```

No changes to configuration are needed if you are using LocalDB.

### Steps

```bash
cd ECommerce.Server/ECommerceAPI
dotnet restore
dotnet run
```

The API will start at:
- `http://localhost:60429`
- `https://localhost:60428`

The Angular frontend is configured to call `http://localhost:60429/api`.

---

## 3. Running the Frontend (Angular)

```bash
cd ECommerce.client
npm install
ng serve
```

The application will be available at `http://localhost:4200`.

---

## 4. Running the Unit Tests

### Backend tests (xUnit)

```bash
cd ECommerce.Server/ECommerceAPI.Tests
dotnet test
```

### Frontend tests (Karma/Jasmine)

```bash
cd ECommerce.client
ng test
```

---

## API Overview

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | /api/auth/register | Register a new user | No |
| POST | /api/auth/login | Login and receive a JWT token | No |
| GET | /api/products | Get all products | No |
| GET | /api/products/{id} | Get a single product | No |
| POST | /api/orders | Place an order | Yes |
| GET | /api/orders | Get orders for the logged-in user | Yes |
| POST | /api/reviews | Submit a product review | Yes |
| GET | /api/reviews/{productId} | Get reviews for a product | No |

Authentication uses JWT Bearer tokens. After logging in, the token is stored in the browser and automatically attached to protected requests via an HTTP interceptor.

---

## Architecture Notes

- The backend uses raw ADO.NET with `Microsoft.Data.SqlClient`. No ORM is used.
- Dependency Injection is configured in `Program.cs` for all repositories and services.
- The total order price is always calculated server-side from the product database during checkout. The price sent by the frontend is ignored.
- Cart state in the frontend is managed via an Angular Service using `BehaviorSubject` (RxJS), so the cart counter updates instantly across all components.
