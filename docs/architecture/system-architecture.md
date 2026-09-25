# System Architecture

## Purpose

This document defines the architectural vision for the Ecommerce Store application.

It serves as the authoritative source for:

- Solution architecture
- Bounded contexts
- Aggregate boundaries
- Layer responsibilities
- Dependency rules
- Domain modeling guidance

All development should align with this document.

---

# Architecture Principles

The solution follows:

- Clean Architecture
- Domain Driven Design (DDD)
- SOLID
- CQRS
- Dependency Injection

Primary goals:

- Maintainability
- Testability
- Scalability
- Separation of Concerns

---

# High Level Architecture

```text
┌──────────────────────┐
│    Blazor Web UI     │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│     REST API         │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│    Application       │
│   CQRS / MediatR     │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│      Domain          │
│ Business Rules       │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│   Infrastructure     │
│ EF Core / External   │
└──────────────────────┘
```

---

# Solution Structure

```text
src/

    Ecommerce.Domain

    Ecommerce.Application

    Ecommerce.Infrastructure

    Ecommerce.Api

    Ecommerce.Web

    Ecommerce.Contracts

tests/

    Ecommerce.Domain.Tests

    Ecommerce.Application.Tests

    Ecommerce.Infrastructure.Tests

    Ecommerce.Api.Tests

    Ecommerce.Architecture.Tests
```

---

# Layer Responsibilities

## Domain

Contains core business rules.

Includes:

- Aggregates
- Entities
- Value Objects
- Domain Events
- Repository Contracts
- Specifications

Must not reference:

- EF Core
- ASP.NET Core
- Blazor
- Infrastructure

---

## Application

Contains use cases.

Includes:

- Commands
- Queries
- Handlers
- Validation
- DTOs
- Application Services

Must not contain:

- EF Core
- Persistence logic
- UI logic

---

## Infrastructure

Contains technical implementations.

Includes:

- EF Core
- Repositories
- Identity
- Caching
- Payment Providers
- Messaging

May depend on:

- Domain
- Application

---

## API

Exposes application functionality.

Responsibilities:

- Request handling
- Authentication
- Authorization
- OpenAPI
- ProblemDetails

Must remain thin.

Business logic belongs elsewhere.

---

## Web

Blazor frontend.

Responsibilities:

- User experience
- State management
- API communication

Business rules belong in backend services.

---

# Bounded Contexts

The application is organized into bounded contexts.

---

## Catalog

Responsible for product management.

### Aggregate Roots

```text
Product
Category
```

### Responsibilities

- Product catalog
- Product search
- Product pricing
- Categories
- Product visibility

### Entities

```text
Product

Category

ProductImage
```

### Value Objects

```text
Money

Sku
```

---

## Customers

Responsible for customer identity.

### Aggregate Root

```text
Customer
```

### Responsibilities

- Customer profile
- Addresses
- Account preferences

### Entities

```text
Customer

CustomerAddress
```

### Value Objects

```text
EmailAddress

PhoneNumber

Address
```

---

## Cart

Responsible for shopping cart management.

### Aggregate Root

```text
ShoppingCart
```

### Responsibilities

- Add items
- Remove items
- Calculate totals

### Entities

```text
ShoppingCart

ShoppingCartItem
```

### Value Objects

```text
Money
```

---

## Orders

Responsible for order processing.

### Aggregate Root

```text
Order
```

### Responsibilities

- Place order
- Track order status
- Manage order lifecycle

### Entities

```text
Order

OrderItem
```

### Value Objects

```text
Address

Money
```

### Domain Events

```text
OrderPlaced

OrderCancelled

OrderCompleted
```

---

## Payments

Responsible for payment processing.

### Aggregate Root

```text
Payment
```

### Responsibilities

- Payment authorization
- Payment capture
- Refund processing

### Domain Events

```text
PaymentSucceeded

PaymentFailed

RefundIssued
```

Initial implementation may defer this context.

---

## Inventory

Responsible for stock management.

### Aggregate Root

```text
InventoryItem
```

### Responsibilities

- Inventory tracking
- Stock reservations
- Reorder monitoring

Initial implementation may defer this context.

---

# Aggregate Design Rules

Aggregates should:

- Protect invariants
- Control child entities
- Expose business behavior

Preferred:

```csharp
order.AddItem(product, quantity);

order.Cancel();

order.Complete();
```

Avoid:

```csharp
order.Items.Add(item);

order.Status = Completed;
```

---

# Domain Events

Use Domain Events to communicate significant business activity.

Examples:

```text
OrderPlaced

PaymentProcessed

InventoryReserved

ProductOutOfStock
```

Events should describe something that already happened.

Good:

```text
OrderPlaced
```

Avoid:

```text
PlaceOrder
```

---

# Repository Strategy

Repositories are defined per aggregate.

Preferred:

```text
IProductRepository

IOrderRepository

ICustomerRepository

IShoppingCartRepository
```

Avoid:

```text
IGenericRepository<T>

IRepository<T>
```

Repositories should expose domain-focused operations.

---

# CQRS Strategy

Application uses CQRS with MediatR.

---

## Commands

Commands mutate state.

Examples:

```text
CreateProductCommand

CreateOrderCommand

CancelOrderCommand
```

---

## Queries

Queries read data.

Examples:

```text
GetProductByIdQuery

GetOrderByIdQuery

GetProductsByCategoryQuery
```

---

# Validation Strategy

FluentValidation is standardized.

Every command should have a validator.

Examples:

```text
CreateOrderCommandValidator

CreateProductCommandValidator
```

Validation should occur before business execution.

---

# Persistence Strategy

Entity Framework Core is the primary persistence technology.

Guidelines:

- One configuration per entity
- Use Fluent API
- Keep DbContext thin
- Use migrations
- Avoid lazy loading

Preferred:

```text
Infrastructure
 └── Persistence
      ├── DbContext
      ├── Configurations
      ├── Repositories
      └── Migrations
```

---

# API Strategy

The API follows REST principles.

Examples:

```text
GET     /api/products

GET     /api/products/{id}

POST    /api/orders

PUT     /api/products/{id}

DELETE  /api/products/{id}
```

Endpoints should:

- Validate input
- Send commands/queries
- Return results

Endpoints should not implement business rules.

---

# Security Strategy

Authentication:

```text
ASP.NET Core Identity
JWT Bearer Tokens
```

Authorization:

```text
Role-based authorization

Policy-based authorization
```

Common Roles:

```text
Admin

Customer
```

---

# Initial MVP Scope

The first release includes:

✅ Products

✅ Categories

✅ Shopping Cart

✅ Orders

✅ Customer Accounts

✅ Authentication

✅ Product Administration

---

# Deferred Features

Not included in MVP:

```text
Inventory Synchronization

Promotions

Coupons

Loyalty Programs

Recommendations

Wish Lists

Multi-Tenant Support

Microservices
```

These should remain out of scope until the core platform is stable.

---

# Architectural Decision Rule

When a design decision is unclear:

1. Protect the domain model.
2. Keep dependencies flowing inward.
3. Prefer explicitness over abstraction.
4. Prefer maintainability over optimization.
5. Favor simple solutions first.

The domain model is the center of the application.