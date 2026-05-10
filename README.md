# TechShop — Fullstack E-Commerce Exercise

A fullstack e-commerce application built with **Angular 19** (frontend) and **ASP.NET Core 8** (backend), using **MS SQL Server** and raw ADO.NET (no ORM).

---

## Project Structure

```
ecommerce/
├── backend/
│   ├── ECommerce.sln
│   ├── database-setup.sql          ← Run this first!
│   ├── ECommerceAPI/               ← ASP.NET Core Web API
│   │   ├── Controllers/
│   │   ├── Services/
│   │   ├── Repositories/
│   │   ├── Models/
│   │   ├── DTOs/
│   │   ├── appsettings.json
│   │   └── Program.cs
│   └── ECommerceAPI.Tests/         ← xUnit tests
│       ├── OrderServiceTests.cs
│       └── AuthServiceTests.cs
└── frontend/
    └── src/app/
        ├── components/
        │   ├── product-list/
        │   ├── cart/
        │   ├── checkout/
        │   ├── register/
        │   ├── login/
        │   └── navbar/
        ├── services/
        │   ├── auth.service.ts
        │   ├── cart.service.ts     ← BehaviorSubject state management
        │   ├── product.service.ts
        │   └── order.service.ts
        ├── models/
        ├── guards/
        └── styles.css
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Node.js 24+ & npm 11+](https://nodejs.org/)
- [Angular CLI 21+](https://angular.io/cli): `npm install -g @angular/cli`
- SQL Server (local instance)

---

## Step 1 — Set Up the Database

Open **SQL Server Management Studio** (or use `sqlcmd`) and run:

```sql
-- From: backend/database-setup.sql
```

This will:
1. Create the `ECommerceDB` database
2. Create tables: `Users`, `Products`, `Orders`, `OrderItems`
3. Seed 12 IT equipment products (laptops, printers, monitors, etc.)

---

## Step 2 — Configure & Run the Backend

### 2a. Update connection string (if needed)

Edit `backend/ECommerceAPI/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "SuperSecretKey_ChangeThisInProduction_MinLength32Chars!",
    "Issuer": "ECommerceAPI",
    "Audience": "ECommerceClient"
  }
}
```

> If you use SQL Server with username/password instead of Windows auth, change to:
> `Server=localhost;Database=ECommerceDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;`

### 2b. Restore packages and run

```bash
cd backend/ECommerceAPI
dotnet restore
dotnet run
```

The API will start at **http://localhost:5000**

OpenAPI spec available at: **http://localhost:5000/openapi/v1.json**

> .NET 10 folosește OpenAPI built-in în loc de Swashbuckle. Poți importa JSON-ul în Postman sau Swagger Editor online.

---

## Step 3 — Run the Frontend

```bash
cd frontend
npm install
ng serve
```

The app will open at **http://localhost:4200**

---

## Step 4 — Run Tests

### Backend (xUnit)

```bash
cd backend
dotnet test
```

### Frontend (Jasmine/Karma)

```bash
cd frontend
ng test
```

---

## API Endpoints

| Method | Endpoint               | Auth Required | Description              |
|--------|------------------------|---------------|--------------------------|
| POST   | /api/auth/register     | No            | Create a new account     |
| POST   | /api/auth/login        | No            | Login, receive JWT token |
| GET    | /api/products          | No            | List all products        |
| GET    | /api/products/{id}     | No            | Get product by ID        |
| POST   | /api/orders/checkout   | Yes (JWT)     | Place an order           |

---

## Key Architecture Decisions

### Backend Price Calculation (Security)
The backend **never trusts the price sent by the frontend**. During checkout (`POST /api/orders/checkout`), the `OrderService` fetches product prices directly from the database and recalculates the total server-side. This prevents price manipulation attacks.

### State Management
The Angular `CartService` uses **RxJS `BehaviorSubject`** to hold cart state. All components subscribe to `items$` and `totalCount` reactively — the navbar cart counter updates instantly when a product is added anywhere in the app.

### No ORM
All database access uses raw **ADO.NET** (`SqlCommand`, `SqlDataReader`) with parameterized queries to prevent SQL injection.

### Dependency Injection
All services and repositories are registered in `Program.cs` using ASP.NET Core's built-in DI container (`AddScoped`, `AddSingleton`).

---

## Notes

- JWT tokens expire after **8 hours**
- Checkout requires authentication (redirects to login if not logged in)
- Browsing products and adding to cart works without login
