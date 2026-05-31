# Developer Evaluation Project

`READ CAREFULLY`

## Use Case
**You are a developer on the DeveloperStore team. Now we need to implement the API prototypes.**

As we work with `DDD`, to reference entities from other domains, we use the `External Identities` pattern with denormalization of entity descriptions.

Therefore, you will write an API (complete CRUD) that handles sales records. The API needs to be able to inform:

* Sale number
* Date when the sale was made
* Customer
* Total sale amount
* Branch where the sale was made
* Products
* Quantities
* Unit prices
* Discounts
* Total amount for each item
* Cancelled/Not Cancelled

It's not mandatory, but it would be a differential to build code for publishing events of:
* SaleCreated
* SaleModified
* SaleCancelled
* ItemCancelled

If you write the code, **it's not required** to actually publish to any Message Broker. You can log a message in the application log or however you find most convenient.

### Business Rules

* Purchases above 4 identical items have a 10% discount
* Purchases between 10 and 20 identical items have a 20% discount
* It's not possible to sell above 20 identical items
* Purchases below 4 items cannot have a discount

These business rules define quantity-based discounting tiers and limitations:

1. Discount Tiers:
   - 4+ items: 10% discount
   - 10-20 items: 20% discount

2. Restrictions:
   - Maximum limit: 20 items per product
   - No discounts allowed for quantities below 4 items

## Overview
This section provides a high-level overview of the project and the various skills and competencies it aims to assess for developer candidates. 

See [Overview](/.doc/overview.md)

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

<!-- 
## API Structure
This section includes links to the detailed documentation for the different API resources:
- [API General](./docs/general-api.md)
- [Products API](/.doc/products-api.md)
- [Carts API](/.doc/carts-api.md)
- [Users API](/.doc/users-api.md)
- [Auth API](/.doc/auth-api.md)
-->

## Project Structure
This section describes the overall structure and organization of the project files and directories. 

See [Project Structure](/.doc/project-structure.md)

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)
- [dotnet-ef CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) — install once:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

### 1. Clone the repository

```bash
git clone <repository-url>
cd template/backend
```

---

### 2. Start the database

```bash
docker-compose up -d ambev.developerevaluation.database
```

> PostgreSQL will be available at `localhost:5432`  
> Database: `developer_evaluation` | User: `developer` | Password: `ev@luAt10n`

---

### 3. Apply database migrations

```bash
dotnet ef database update \
  --project src/Ambev.DeveloperEvaluation.ORM \
  --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

---

### 4. Run the API

```bash
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

The API will start at `http://localhost:5119` (or the port shown in the terminal).

---

### 5. Explore with Swagger

Open your browser at:

```
http://localhost:5119/swagger
```

---

## Sales API — Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/sales` | Create a new sale |
| `GET` | `/api/sales` | List sales (paginated) |
| `GET` | `/api/sales/{id}` | Get sale by ID |
| `PUT` | `/api/sales/{id}` | Update a sale |
| `DELETE` | `/api/sales/{id}` | Delete a sale |
| `PATCH` | `/api/sales/{id}/cancel` | Cancel a sale |
| `PATCH` | `/api/sales/{id}/items/{itemId}/cancel` | Cancel a sale item |

### Pagination & Ordering (GET /api/sales)

| Parameter | Default | Description |
|-----------|---------|-------------|
| `_page` | 1 | Page number |
| `_size` | 10 | Items per page |
| `_order` | `saleDate desc` | Field and direction, e.g. `"totalAmount desc, saleDate asc"` |

### Example: Create Sale

```json
POST /api/sales
{
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
    }
  ]
}
```

> With `quantity: 10` — a **20% discount** is automatically applied.

### Business Rules (enforced automatically)

| Quantity | Discount |
|----------|----------|
| 1–3 | 0% |
| 4–9 | 10% |
| 10–20 | 20% |
| > 20 | ❌ Not allowed |

---

## Running Tests

```bash
dotnet test tests/Ambev.DeveloperEvaluation.Unit
```

To see detailed test output:

```bash
dotnet test tests/Ambev.DeveloperEvaluation.Unit --logger "console;verbosity=detailed"
```

### Test coverage includes:
- `SaleItem` discount calculation for all quantity tiers
- `Sale` aggregate: total recalculation, cancellation, item management
- `CreateSaleHandler`: success, duplicate number, validation error, event publishing
- `GetSaleHandler`: found and not found
- `CancelSaleHandler` / `CancelSaleItemHandler`: all scenarios

---

## Domain Events

Events are published via MediatR and logged to the application log (no broker required):

| Event | Trigger |
|-------|---------|
| `SaleCreated` | POST /api/sales |
| `SaleModified` | PUT /api/sales/{id} |
| `SaleCancelled` | PATCH /api/sales/{id}/cancel |
| `ItemCancelled` | PATCH /api/sales/{id}/items/{itemId}/cancel |

---

## Running with Full Docker Stack

To run the API and database together:

```bash
docker-compose up --build
```
