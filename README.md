# TechForge ⚙️

A modern, dark-themed **e-commerce platform for computer parts, peripherals, laptops and tech products** — built with **ASP.NET Core 8.0 MVC**, **Entity Framework Core**, **SQL Server**, and **ASP.NET Identity**.

Think Newegg / PCPartPicker / Micro Center, in a neon-on-black gamer aesthetic — with a full storefront, a **PC Builder**, a **REST API**, **AJAX** interactions, and a complete **admin back-office**.

---

## Table of contents
- [Tech stack](#tech-stack)
- [Features](#features)
- [Solution architecture](#solution-architecture)
- [Prerequisites](#prerequisites)
- [Getting started](#getting-started)
- [Default accounts](#default-accounts)
- [Running the tests](#running-the-tests)
- [Security](#security)
- [Requirement checklist](#requirement-checklist)
- [Notes](#notes)

---

## Tech stack

| Layer | Technology |
|-------|------------|
| **Web** | ASP.NET Core 8.0 MVC, Razor Views, View Components, Bootstrap 5, vanilla JS (Fetch/AJAX) |
| **Data** | Entity Framework Core 8 (Code-First, Migrations), SQL Server (Docker) |
| **Auth** | ASP.NET Identity with **roles** (Admin / Customer) + an authorization **policy** |
| **Mapping** | AutoMapper |
| **Tests** | NUnit 4 + Moq + EF Core InMemory + coverlet (**94 % coverage**) |
| **Tooling** | Visual Studio 2022, .NET 8 SDK, Docker Desktop |

---

## Features

### Public (no login required)
- Home page with **featured deals**, **new arrivals** and **shop-by-category** sections
- **Product catalog** with a filter sidebar: search, category, brand, price range, in-stock toggle
- **Sorting** (Newest, Price ↑/↓, Top Rated, Most Popular, Name A–Z) and **pagination**
- **Product details**: image, specifications, average rating, reviews, related products
- **PC Builder** — pick one component per category, see a **live running total**, add the whole build to the cart
- **Live product search** in the navbar (AJAX, instant dropdown)
- Custom **error pages** (400 / 401 / 403 / 404 / 500)

### Authenticated customers
- **Register / Login / Logout** and **profile** management
- **Shopping cart** with persistence (saved to the DB per user) and a live navbar badge
- **Wishlist** (heart toggle anywhere → wishlist page)
- **Checkout** → places an order, **decrements stock**, clears the cart
- **Order history** and order details
- Write & delete **product reviews** (one per product; rating auto-recalculates)
- **Add-to-cart / wishlist work without a page refresh** (AJAX + toast notifications)

### Admin (Admin role only)
- **Dashboard** with animated stats (products, orders, users, revenue), recent orders, low-stock alerts
- **Manage Products** — full CRUD with validation + category picker
- **Manage Categories** — full CRUD (with delete-guards)
- **Manage Orders** — view all, update status inline
- **Manage Users** — list users and their roles
- **Manage Reviews** — moderate / delete any review

---

## Solution architecture

A clean, layered, multi-project solution. Each project only depends on the ones below it.

```
TechForge/
├── TechForge.sln
├── TechForge.Core/          # Domain layer — no infrastructure dependencies
│   ├── Entities/            #   ApplicationUser, Product, Category, Review, Order, OrderItem, CartItem, WishlistItem
│   ├── Enums/               #   OrderStatus
│   ├── Dtos/                #   read & input DTOs returned by services
│   └── Querying/            #   PagedResult<T>, ProductQueryOptions, ProductSortOption
│
├── TechForge.Data/          # Data-access layer
│   ├── ApplicationDbContext #   EF Core context (Identity + domain) with Fluent API config
│   ├── Migrations/          #   EF Core migrations
│   └── Seeding/             #   DbInitializer — roles, admin, demo users, 8 categories, 12 products, reviews
│
├── TechForge.Services/      # Business-logic layer
│   ├── Contracts/           #   IProductService, ICartService, IOrderService, IReviewService, ...
│   ├── Implementations/     #   the concrete services (inject ApplicationDbContext + IMapper)
│   └── Mapping/             #   AutoMapper MappingProfile
│
├── TechForge.Web/           # Presentation layer (MVC host)
│   ├── Controllers/         #   Home, Products, Cart, Wishlist, Orders, Reviews, Account, Admin, Builder
│   │   └── Api/             #   ProductApiController (REST/JSON)
│   ├── ViewComponents/      #   CartSummary (navbar badge)
│   ├── Middleware/          #   ExceptionHandlingMiddleware (global exception handling)
│   ├── ViewModels/          #   form & page models
│   ├── Views/               #   Razor views + dark Bootstrap 5 theme
│   └── wwwroot/             #   site.css, site.js, builder.js
│
└── TechForge.Tests/         # NUnit + Moq test project (94 % coverage)
```

**Reference chain:** `Web → Services → Data → Core` (and `Tests → Services/Data/Core`).

**On the repository pattern:** the service layer talks to EF Core's `ApplicationDbContext` directly. In EF Core, the **`DbContext` is the Unit of Work** and each **`DbSet<T>` is a repository** — so this *is* the repository pattern, just using EF's built-in implementation rather than a hand-rolled wrapper. Business logic lives in the services behind interfaces (`Contracts/`), keeping controllers thin and everything mockable.

---

## Prerequisites

- **Visual Studio 2022** (17.8+) with the *ASP.NET and web development* workload — or the **.NET 8 SDK** + your editor of choice
- **Docker Desktop** (for the SQL Server container) — or a local/remote SQL Server instance

---

## Getting started

### 1. Start SQL Server (Docker)

The app expects SQL Server on `localhost,1433`. The first time, create the container:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SuperStrongPass!23" \
  -p 1433:1433 -d --name techforge-sql \
  mcr.microsoft.com/mssql/server:2022-latest
```

On later runs, just start it:

```bash
docker start techforge-sql
```

> The connection string lives in `TechForge.Web/appsettings.json` (`ConnectionStrings:DefaultConnection`). Change it there if you use a different server/password.

### 2. Run the app

```bash
cd TechForge
dotnet restore
dotnet run --project TechForge.Web
```

On first launch the app automatically **applies migrations** and **seeds** the database (roles, accounts, categories, products, reviews).

Then open **https://localhost:7050** (or http://localhost:5050).

> ⚠️ Run from **either** Visual Studio **or** the terminal — not both at once, or you'll get a file-lock error on `TechForge.Web.exe`.

---

## Default accounts

Seeded automatically on first run:

| Role | Email | Password |
|------|-------|----------|
| **Admin** | `admin@techforge.local` | `Admin#12345` |
| Customer | `alex@demo.local` | `Demo#12345` |
| Customer | `jordan@demo.local` | `Demo#12345` |
| Customer | `sam@demo.local` | `Demo#12345` |

The login page also lists these for convenience.

---

## Running the tests

```bash
# run all tests
dotnet test

# run with code coverage (excludes auto-generated migrations)
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

Or in Visual Studio: **Test → Run All Tests** (Test Explorer).

**70 NUnit tests** cover the Product, Cart, Order, Review, Category and Wishlist services plus the database seeder, using an **EF Core InMemory** database and a real AutoMapper. **Moq** is used to mock `IMapper` and verify interactions.

| Assembly | Line coverage |
|----------|---------------|
| TechForge.Core | 100 % |
| TechForge.Data | 98.8 % |
| TechForge.Services | 88.4 % |
| **Overall** | **94 %** |

---

## Security

- **ASP.NET Identity** for authentication; passwords hashed; account lockout after 5 failed attempts
- **Role-based authorization** + a named **`AdminOnly` policy** protecting the entire admin area
- **CSRF protection** — `[ValidateAntiForgeryToken]` on every state-changing POST
- **XSS prevention** — Razor output-encoding by default
- **SQL-injection safe** — all access via parameterized EF Core LINQ
- **Open-redirect protection** — `Url.IsLocalUrl` / same-host checks on returnUrl & referrers
- **Server-side + client-side validation** via data annotations and jQuery unobtrusive validation
- AJAX/API requests receive proper **401/403** responses instead of HTML redirects

---

## Requirement checklist

| Requirement | Where |
|-------------|-------|
| ASP.NET Core 8 MVC | `TechForge.Web` |
| SQL Server + EF Core + migrations + seeded data | `TechForge.Data` |
| ≥ 4 entity models | 8 entities in `Core/Entities` |
| ≥ 4 controllers | Home, Products, Cart, Wishlist, Orders, Reviews, Account, Admin, Builder |
| 1 API controller | `Controllers/Api/ProductApiController` |
| ≥ 10 views | catalog, details, cart, wishlist, checkout, orders, login, register, profile, admin pages, builder… |
| CRUD | Admin products & categories |
| AJAX | add-to-cart, wishlist toggle, live search, PC Builder |
| Dependency Injection | `AddApplicationServices()` + constructor injection throughout |
| ASP.NET Identity with roles | Admin / Customer + `AdminOnly` policy |
| Custom error pages | `Views/Error/{400,401,403,404,500}` + global middleware |
| NUnit + Moq unit tests, ~70 % coverage | `TechForge.Tests` (**94 %**) |
| AutoMapper | `Services/Mapping/MappingProfile` |
| Multi-project architecture | Core / Data / Services / Web / Tests |
| Responsive modern UI | dark Bootstrap 5 theme, gradients, glass cards |
| **Bonus:** PC Builder | `BuilderController` + `builder.js` + REST API |

---

Built for makers, gamers, and engineers. 🛠️
