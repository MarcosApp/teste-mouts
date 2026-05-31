# Developer Evaluation Project

## Use Case

You are a developer on the **DeveloperStore** team implementing API prototypes using DDD principles. Entities from other domains are referenced via the **External Identities** pattern with denormalized descriptions.

This API handles complete **Sales records** and also implements the **Products**, **Users**, and **Auth** APIs.

### Business Rules

| Quantity per item | Discount |
|-------------------|----------|
| 1–3 | 0% (no discount) |
| 4–9 | 10% |
| 10–20 | 20% |
| > 20 | ❌ Not allowed |

### Domain Events (logged via ILogger + MediatR)

| Event | Trigger |
|-------|---------|
| `SaleCreated` | POST /api/sales |
| `SaleModified` | PUT /api/sales/{id} |
| `SaleCancelled` | PATCH /api/sales/{id}/cancel |
| `ItemCancelled` | PATCH /api/sales/{id}/items/{itemId}/cancel |

---

## Tech Stack

| Technology | Role |
|-----------|------|
| **.NET 8 / C#** | Backend framework |
| **PostgreSQL 13** | Relational database — Users, Sales, Products |
| **MongoDB 8.0** | Document database — Carts (NoSQL) |
| **EF Core 8** | ORM for PostgreSQL with code-first migrations |
| **MongoDB.Driver 2.28** | Official MongoDB .NET driver |
| **MediatR 12** | CQRS — Commands, Queries, Handlers |
| **AutoMapper 13** | Object mapping between layers |
| **FluentValidation 11** | Input validation |
| **xUnit + NSubstitute + Bogus** | Unit testing, mocking, fake data |
| **Serilog** | Structured logging |
| **Docker / Docker Compose** | Containerization |

---

## Project Structure

```
template/backend/
├── src/
│   ├── Ambev.DeveloperEvaluation.Domain/
│   │   ├── Entities/         # Sale, SaleItem, User, Product, Cart
│   │   ├── ValueObjects/     # Rating, UserName, Address, Geolocation
│   │   ├── Events/           # SaleCreated, SaleModified, SaleCancelled, ItemCancelled
│   │   └── Repositories/     # ISaleRepository, IUserRepository, IProductRepository, ICartRepository
│   │
│   ├── Ambev.DeveloperEvaluation.Application/
│   │   ├── Sales/            # CreateSale, GetSale, UpdateSale, DeleteSale, CancelSale, ListSales
│   │   ├── Products/         # CreateProduct, GetProduct, UpdateProduct, DeleteProduct, ListProducts, GetCategories
│   │   ├── Users/            # CreateUser, GetUser, GetUsers, UpdateUser, DeleteUser
│   │   ├── Carts/            # CreateCart, GetCart, UpdateCart, DeleteCart, ListCarts
│   │   └── Auth/             # AuthenticateUser
│   │
│   ├── Ambev.DeveloperEvaluation.ORM/
│   │   ├── Mapping/          # EF Core configurations (Sale, SaleItem, User, Product)
│   │   ├── Migrations/       # PostgreSQL migrations
│   │   ├── Repositories/     # SaleRepository, UserRepository, ProductRepository (EF Core)
│   │   │                     # CartRepository (MongoDB)
│   │   ├── DefaultContext.cs # EF Core DbContext (PostgreSQL)
│   │   ├── MongoDbContext.cs # MongoDB context
│   │   └── MongoDbSetup.cs   # BSON conventions and class maps
│   │
│   ├── Ambev.DeveloperEvaluation.Common/  # JWT, BCrypt, Validation
│   ├── Ambev.DeveloperEvaluation.IoC/     # DI registration (EF Core + MongoDB)
│   └── Ambev.DeveloperEvaluation.WebApi/
│       ├── Features/         # Controllers per feature (Sales, Products, Users, Carts, Auth)
│       └── Middleware/       # Exception handling, 401/404/400/500 mapping
│
└── tests/
    ├── Ambev.DeveloperEvaluation.Unit/    # 82 unit tests
    ├── Ambev.DeveloperEvaluation.Integration/
    └── Ambev.DeveloperEvaluation.Functional/
```

---

## Getting Started

### Prerequisites

| Tool | Version | Install |
|------|---------|---------|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download |
| Docker Desktop | Latest | https://docs.docker.com/get-docker/ |
| dotnet-ef CLI | Latest | `dotnet tool install --global dotnet-ef` |

---

## Option 1 — Full Docker Stack (Recommended)

Runs the API + PostgreSQL together in containers.

```bash
# 1. Clone the repository
git clone https://github.com/MarcosApp/teste-mouts.git
cd teste-mouts/template/backend

# 2. Build and start all services
docker-compose up --build
```

The API will be available at **http://localhost:8080**.

> Swagger UI: **http://localhost:8080/swagger**

### Services started by Docker Compose

| Service | Port | Credentials |
|---------|------|-------------|
| WebApi | 8080 (HTTP), 8081 (HTTPS) | — |
| PostgreSQL | 5432 | user: `developer` / pass: `ev@luAt10n` / db: `developer_evaluation` |
| MongoDB | 27017 | user: `developer` / pass: `ev@luAt10n` |
| Redis | 6379 | pass: `ev@luAt10n` |

---

## Option 2 — Local Development

```bash
cd teste-mouts/template/backend

# 1. Start only the database
docker-compose up -d ambev.developerevaluation.database

# 2. Apply all migrations
dotnet ef database update \
  --project src/Ambev.DeveloperEvaluation.ORM \
  --startup-project src/Ambev.DeveloperEvaluation.WebApi \
  --configuration Release

# 3. Run the API
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

The API will be available at **http://localhost:5119**.

> Swagger UI: **http://localhost:5119/swagger**

---

## Environment Variables

The connection string and JWT secret can be overridden via environment variables:

| Variable | Default (appsettings.json) | Description |
|----------|---------------------------|-------------|
| `ConnectionStrings__DefaultConnection` | `Host=localhost;Port=5432;Database=developer_evaluation;Username=developer;Password=ev@luAt10n` | PostgreSQL connection string |
| `Jwt__SecretKey` | `YourSuperSecretKeyForJwtTokenGenerationThatShouldBeAtLeast32BytesLong` | JWT signing key |
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Development` enables Swagger |

**Example override (local):**
```bash
export ConnectionStrings__DefaultConnection="Host=myhost;Port=5432;Database=mydb;Username=myuser;Password=mypass"
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

**In docker-compose the WebApi service already sets:**
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=ambev.developerevaluation.database;Port=5432;...
```

---

## Database Architecture — PostgreSQL + MongoDB

This project uses **two databases**, each chosen for what it does best.

### Why two databases?

| | PostgreSQL | MongoDB |
|-|-----------|---------|
| **Used for** | Users, Sales, Products | Carts |
| **Type** | Relational (ACID transactions) | Document (NoSQL, schema-flexible) |
| **Access layer** | EF Core + Migrations | MongoDB.Driver |
| **Why it fits** | Sales/Users have fixed structure, need joins, enforce business rules | Carts have variable product lists per user, no joins needed, structure changes per context |

> The project spec (`tech-stack.md`) explicitly lists both databases. The evaluation criteria (`overview.md`) assess *"Database skills with both PostgreSQL and MongoDB"* and *"Understanding of both relational and non relational database systems"*.

---

### PostgreSQL Configuration

**Connection string** (`appsettings.json`):
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=developer_evaluation;Username=developer;Password=ev@luAt10n"
}
```

**Apply migrations** (creates tables: Users, Sales, SaleItems, Products):
```bash
dotnet ef database update \
  --project src/Ambev.DeveloperEvaluation.ORM \
  --startup-project src/Ambev.DeveloperEvaluation.WebApi \
  --configuration Release
```

**Tables managed by EF Core:**
| Table | Entity | Notes |
|-------|--------|-------|
| `Users` | `User` | Name + Address as owned value objects |
| `Sales` | `Sale` | Aggregate root with Items |
| `SaleItems` | `SaleItem` | Child of Sale, cascade delete |
| `Products` | `Product` | Rating as owned value object |

---

### MongoDB Configuration

**Connection string** (`appsettings.json`):
```json
"ConnectionStrings": {
  "MongoConnection": "mongodb://developer:ev%40luAt10n@localhost:27017/developer_evaluation?authSource=admin"
}
```

**No migrations needed** — MongoDB creates the collection automatically on first insert.

**Collection:** `carts`

**Document structure:**
```json
{
  "_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
  "date": "2026-05-31T00:00:00Z",
  "products": [
    { "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa8", "quantity": 2 },
    { "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa9", "quantity": 5 }
  ]
}
```

**Technical implementation decisions:**

| Decision | Reason |
|----------|--------|
| `GuidSerializer(BsonType.String)` | Stores Guid as readable string in MongoDB instead of binary — easier to read in DB tools (Compass, Studio 3T) |
| `BsonClassMap` for `Cart` | Keeps MongoDB config in the ORM layer — no infrastructure attributes (`[BsonId]`) leaking into the Domain entity |
| `CamelCaseElementNameConvention` | JSON-style field names in MongoDB documents (`userId`, `products`) consistent with the API |
| `IgnoreExtraElementsConvention` | Prevents deserialization errors if the document has extra fields — supports schema evolution |
| `MongoDbContext` as `Singleton` | MongoDB driver is thread-safe; a single context instance is optimal |

**Start MongoDB locally:**
```bash
docker-compose up -d ambev.developerevaluation.nosql
```

**Verify connection** — after starting the API:
```bash
curl http://localhost:5119/api/carts
# Returns: { "data": [], "totalItems": 0, "currentPage": 1, "totalPages": 0 }
```

---

### Carts API

Full CRUD for shopping carts stored in MongoDB.

#### Create Cart
```bash
curl -X POST http://localhost:5119/api/carts \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "date": "2026-05-31T00:00:00Z",
    "products": [
      { "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa7", "quantity": 2 },
      { "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa8", "quantity": 5 }
    ]
  }'
```

#### List Carts (paginated)
```bash
curl "http://localhost:5119/api/carts?_page=1&_size=10&_order=date desc"
```

#### Get Cart by ID
```bash
curl "http://localhost:5119/api/carts/{id}"
```

#### Update Cart
```bash
curl -X PUT "http://localhost:5119/api/carts/{id}" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "date": "2026-06-01T00:00:00Z",
    "products": [
      { "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa9", "quantity": 10 }
    ]
  }'
```

#### Delete Cart
```bash
curl -X DELETE "http://localhost:5119/api/carts/{id}"
```

---

## Running Tests

```bash
cd template/backend

# Run all unit tests
dotnet test tests/Ambev.DeveloperEvaluation.Unit

# With detailed output
dotnet test tests/Ambev.DeveloperEvaluation.Unit --logger "console;verbosity=detailed"

# With coverage report
dotnet test tests/Ambev.DeveloperEvaluation.Unit /p:CollectCoverage=true /p:CoverletOutputFormat=lcov
```

**82 tests — 0 failures.** Coverage includes:

| Test Class | What is tested |
|------------|----------------|
| `SaleItemTests` | Discount tiers (0%, 10%, 20%) and max-quantity guard (> 20) |
| `SaleTests` | Total recalculation, cancellation, SetItems, Validate() |
| `CreateSaleHandlerTests` | Success, duplicate number, invalid command, event publishing |
| `GetSaleHandlerTests` | Found and not-found paths |
| `CancelSaleHandlerTests` | Cancel sale and cancel item — success, already cancelled, not found |

---

## API Reference

### Authentication

```bash
# Login and get JWT token
curl -X POST http://localhost:5119/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "your_username", "password": "your_password"}'
```

---

### Sales API

#### Create Sale
```bash
curl -X POST http://localhost:5119/api/sales \
  -H "Content-Type: application/json" \
  -d '{
    "saleNumber": "SALE-001",
    "saleDate": "2026-05-31T00:00:00Z",
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "customerName": "John Doe",
    "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
    "branchName": "Main Branch",
    "items": [
      {
        "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
        "productName": "Product A",
        "quantity": 10,
        "unitPrice": 25.00
      },
      {
        "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa9",
        "productName": "Product B",
        "quantity": 3,
        "unitPrice": 100.00
      }
    ]
  }'
```

> Product A: 10 × R$25 with **20% discount** = **R$200**
> Product B: 3 × R$100 with **no discount** = **R$300**
> **Total: R$500**

#### List Sales (paginated + filtered)
```bash
# Basic list
curl "http://localhost:5119/api/sales?_page=1&_size=10"

# Filter by customer
curl "http://localhost:5119/api/sales?customerName=John*"

# Filter by date range
curl "http://localhost:5119/api/sales?_minDate=2026-01-01&_maxDate=2026-12-31"

# Only active sales, ordered by date descending
curl "http://localhost:5119/api/sales?isCancelled=false&_order=saleDate+desc"

# Filter by amount range
curl "http://localhost:5119/api/sales?_minTotalAmount=100&_maxTotalAmount=1000"
```

**Response format (list):**
```json
{
  "data": [ { "id": "...", "saleNumber": "SALE-001", ... } ],
  "totalItems": 10,
  "currentPage": 1,
  "totalPages": 1
}
```

#### Get Sale by ID
```bash
curl "http://localhost:5119/api/sales/{id}"
```

#### Update Sale
```bash
curl -X PUT http://localhost:5119/api/sales/{id} \
  -H "Content-Type: application/json" \
  -d '{
    "saleNumber": "SALE-001-UPDATED",
    "saleDate": "2026-06-01T00:00:00Z",
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "customerName": "John Doe",
    "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
    "branchName": "Main Branch",
    "items": [
      { "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa8", "productName": "Product A", "quantity": 5, "unitPrice": 25.00 }
    ]
  }'
```

#### Cancel Sale
```bash
curl -X PATCH "http://localhost:5119/api/sales/{id}/cancel"
```

#### Cancel Sale Item
```bash
curl -X PATCH "http://localhost:5119/api/sales/{id}/items/{itemId}/cancel"
```

#### Delete Sale
```bash
curl -X DELETE "http://localhost:5119/api/sales/{id}"
```

---

### Products API

#### Create Product
```bash
curl -X POST http://localhost:5119/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Fjallraven Backpack",
    "price": 109.95,
    "description": "Your perfect pack for everyday use.",
    "category": "backpacks",
    "image": "https://example.com/img.jpg",
    "rating": { "rate": 3.9, "count": 120 }
  }'
```

#### List Products
```bash
curl "http://localhost:5119/api/products?_page=1&_size=10&_order=price+desc"
```

#### Get Categories
```bash
curl "http://localhost:5119/api/products/categories"
```

#### Products by Category
```bash
curl "http://localhost:5119/api/products/category/backpacks?_page=1&_size=5"
```

---

### Users API

#### Create User
```bash
curl -X POST http://localhost:5119/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "username": "johndoe",
    "email": "john@example.com",
    "password": "Test@123",
    "phone": "+5511999999999",
    "status": "Active",
    "role": "Customer"
  }'
```

#### List Users
```bash
curl "http://localhost:5119/api/users?_page=1&_size=10&_order=username+asc"
```

---

## Error Response Format

```json
{
  "type": "ResourceNotFound",
  "error": "Sale with ID '...' not found.",
  "detail": "Sale with ID '...' not found."
}
```

| HTTP Status | Type | When |
|-------------|------|------|
| 400 | `ValidationError` | Invalid input (FluentValidation) |
| 400 | `BusinessError` | Business rule violation |
| 404 | `ResourceNotFound` | Entity not found |
| 500 | `InternalError` | Unexpected server error |

---

## Git Flow & Branching Strategy

This project follows the **Git Flow** branching model.

### Branch Structure

```
main         ──────────────────────────────────────────── (production-ready)
                                                    ↑
                                             merge release
                                                    │
develop      ──────────────────────────────────────── (integration)
                ↑            ↑             ↑
         feature/sale  feature/products  feature/users
```

### Branch Types

| Branch | Purpose | Base | Merge into |
|--------|---------|------|------------|
| `main` | Production-ready code only | — | — |
| `develop` | Integration of all features | `main` | `main` (via release) |
| `feature/*` | New feature development | `develop` | `develop` |
| `release/*` | Release preparation and versioning | `develop` | `main` + `develop` |
| `hotfix/*` | Critical production fixes | `main` | `main` + `develop` |

### Workflow for a New Feature

```bash
# 1. Start from develop
git checkout develop
git pull origin develop

# 2. Create feature branch
git checkout -b feature/sale-cancellation

# 3. Develop with semantic commits
git commit -m "feat(domain): add cancellation business rules to SaleItem"
git commit -m "feat(application): add CancelSaleItem handler"
git commit -m "test: add unit tests for CancelSaleItem"

# 4. Push and open Pull Request → develop
git push origin feature/sale-cancellation
# Open PR: feature/sale-cancellation → develop

# 5. After approval, merge into develop
```

### Semantic Commit Convention

This project uses **Conventional Commits**:

```
<type>(<scope>): <description>

Types:
  feat     → new feature
  fix      → bug fix
  refactor → code change without new feature or fix
  test     → adding or updating tests
  docs     → documentation only
  chore    → build process, dependency updates
```

**Examples from this project:**
```
feat(domain): add Sale and SaleItem entities with business rules
feat(application): add CQRS handlers for Sale CRUD and cancellation
feat(orm): add Sale EF Core configuration, repository and migration
feat(webapi): add SalesController with full CRUD endpoints
test: add unit tests for Sale domain rules and application handlers
fix(auth): fix missing AutoMapper mapping for AuthenticateUser
refactor(webapi): fix response nesting and separate request models
docs: rewrite README with full Docker, env vars, curl examples
```

### Feature Branches Implemented

| Branch | Feature | Status |
|--------|---------|--------|
| `feature/sale-domain` | Sale + SaleItem entities, domain events, validators | ✅ Merged |
| `feature/sale-application` | CQRS handlers (Create, Get, Update, Delete, Cancel) | ✅ Merged |
| `feature/sale-orm` | EF Core config, SaleRepository, migrations | ✅ Merged |
| `feature/sale-webapi` | SalesController, request/response models | ✅ Merged |
| `feature/sale-tests` | Unit tests for domain and handlers (82 tests) | ✅ Merged |
| `feature/products-api` | Full Products CRUD + Rating value object | ✅ Merged |
| `feature/users-api` | Complete Users API with name/address | ✅ Merged |
| `feature/sale-filters` | Filtering support for GET /api/sales | ✅ Merged |

---

## Test Results & Validation Report

### API Endpoint Tests — 35 / 35 PASS

| API | Endpoint | Status |
|-----|----------|--------|
| **Auth** | `POST /api/Auth` (valid credentials) | ✅ 200 |
| **Auth** | `POST /api/Auth` (wrong password) | ✅ 401 |
| **Users** | `POST /api/users` | ✅ 201 |
| **Users** | `GET /api/users` (paginated) | ✅ 200 |
| **Users** | `GET /api/users/{id}` | ✅ 200 |
| **Users** | `GET /api/users/{id}` (not found) | ✅ 404 |
| **Users** | `PUT /api/users/{id}` (name + address) | ✅ 200 |
| **Users** | `DELETE /api/users/{id}` | ✅ 200 |
| **Products** | `POST /api/products` (Rating value object) | ✅ 201 |
| **Products** | `GET /api/products` | ✅ 200 |
| **Products** | `GET /api/products/categories` | ✅ 200 |
| **Products** | `GET /api/products/category/{cat}` | ✅ 200 |
| **Products** | `GET /api/products/{id}` | ✅ 200 |
| **Products** | `GET /api/products/{id}` (not found) | ✅ 404 |
| **Products** | `PUT /api/products/{id}` | ✅ 200 |
| **Products** | `DELETE /api/products/{id}` | ✅ 200 |
| **Sales** | `POST /api/sales` (discount tiers applied) | ✅ 201 |
| **Sales** | `POST /api/sales` (duplicate number) | ✅ 400 |
| **Sales** | `POST /api/sales` (qty > 20 rejected) | ✅ 400 |
| **Sales** | `GET /api/sales` (paginated) | ✅ 200 |
| **Sales** | `GET /api/sales?isCancelled=false` | ✅ 200 |
| **Sales** | `GET /api/sales?customerName=Ana*` (wildcard) | ✅ 200 |
| **Sales** | `GET /api/sales?_minTotalAmount=100` | ✅ 200 |
| **Sales** | `GET /api/sales?_minDate=2026-01-01T00:00:00Z` | ✅ 200 |
| **Sales** | `GET /api/sales?_order=totalAmount desc` | ✅ 200 |
| **Sales** | `GET /api/sales/{id}` | ✅ 200 |
| **Sales** | `GET /api/sales/{id}` (not found) | ✅ 404 |
| **Sales** | `PUT /api/sales/{id}` (qty=4 → 10% discount) | ✅ 200 |
| **Sales** | `PATCH /api/sales/{id}/items/{itemId}/cancel` | ✅ 200 |
| **Sales** | `PATCH cancel already-cancelled item` | ✅ 400 |
| **Sales** | `PATCH /api/sales/{id}/cancel` | ✅ 200 |
| **Sales** | `PATCH cancel already-cancelled sale` | ✅ 400 |
| **Sales** | `DELETE /api/sales/{id}` | ✅ 200 |
| **Health** | `GET /health` | ✅ 200 |

### Business Rules — All Verified in Production

| Quantity per item | Discount | Verified |
|-------------------|----------|----------|
| 1–3 | 0% | ✅ `qty=3, price=100 → total R$300` |
| 4–9 | 10% | ✅ `qty=4, price=100 → total R$360` |
| 10–20 | 20% | ✅ `qty=10, price=50 → total R$400` |
| > 20 | ❌ Not allowed | ✅ `qty=21 → HTTP 400` |
| Mixed in one sale | Both tiers | ✅ `R$300 + R$400 = R$700` |

### Unit Tests — 82 / 82 PASS

```
dotnet test tests/Ambev.DeveloperEvaluation.Unit
→ Passed: 82  Failed: 0  Duration: ~1s
```

| Test Class | Tests | Coverage |
|------------|-------|---------|
| `SaleItemTests` | 6 | Discount tiers, max-qty guard, cancellation, total calc |
| `SaleTests` | 5 | Total recalc, cancellation, SetItems, Validate |
| `CreateSaleHandlerTests` | 4 | Success, duplicate number, validation error, event publish |
| `GetSaleHandlerTests` | 2 | Found and not-found paths |
| `CancelSaleHandlerTests` | 4 | Cancel sale/item, already-cancelled, not found |
| `CancelSaleItemHandlerTests` | 3 | Cancel item, already-cancelled, not found |
| `UserTests` + validators | 58 | Template existing tests |

### Architecture Overview

```
Domain      → Entities, Value Objects, Domain Events, Repository interfaces
Application → CQRS Handlers, Validators, AutoMapper Profiles
ORM         → EF Core Configurations, Migrations, Repository implementations
WebApi      → Controllers, Request/Response models, Exception Middleware
IoC         → Dependency injection registration
```

**Key patterns applied:**
- Domain-Driven Design (DDD) with `Sale` aggregate root
- CQRS via MediatR (Command/Query separation)
- External Identity pattern (CustomerId + CustomerName denormalized)
- Value Objects: `Rating`, `UserName`, `Address`, `Geolocation`
- Repository pattern with interfaces in Domain layer
- `ValidationBehavior<,>` pipeline for cross-cutting validation
- `JsonStringEnumConverter` — enums as strings in JSON (`"Active"`, `"Admin"`)

**Error response format:**
```json
{ "type": "ResourceNotFound", "error": "Sale with ID '...' not found.", "detail": "..." }
```

| HTTP | Type | When |
|------|------|------|
| 400 | `ValidationError` | FluentValidation failure |
| 400 | `BusinessError` | Business rule violation |
| 401 | `AuthenticationError` | Invalid credentials |
| 404 | `ResourceNotFound` | Entity not found |
| 500 | `InternalError` | Unexpected server error (logged) |

---

## Documentation References

- [Overview](.doc/overview.md)
- [Tech Stack](.doc/tech-stack.md)
- [Frameworks](.doc/frameworks.md)
- [General API](.doc/general-api.md)
- [Products API](.doc/products-api.md)
- [Carts API](.doc/carts-api.md)
- [Users API](.doc/users-api.md)
- [Auth API](.doc/auth-api.md)
