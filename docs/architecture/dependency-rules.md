# Dependency Rules

## Purpose

This document defines the allowed dependencies and architectural boundaries for the Ecommerce Store solution.

The purpose of these rules is to:

- Preserve Clean Architecture
- Prevent architectural drift
- Protect the domain model
- Maintain separation of concerns
- Enable architecture validation tests

All project references must comply with these rules.

---

# Architectural Principle

Dependencies flow inward.

Business rules should never depend on frameworks or infrastructure implementations.

```text
              Infrastructure
                     │
                     ▼

API  ───────▶  Application  ───────▶ Domain

                     ▲
                     │

               Infrastructure


Web ─────▶ Contracts
                 │
                 ▼
                API
```

The Domain layer is the center of the architecture.

Everything depends on the Domain.

The Domain depends on nothing.

---

# Project Dependency Diagram

```text
Ecommerce.Domain
    ↑

Ecommerce.Application
    ↑

Ecommerce.Infrastructure
    ↑

Ecommerce.Api

Ecommerce.Web
    ↓

Ecommerce.Contracts
```

---

# Allowed Project References

## Ecommerce.Domain

Allowed References:

```text
.NET Runtime
```

Forbidden References:

```text
Ecommerce.Application

Ecommerce.Infrastructure

Ecommerce.Api

Ecommerce.Web

Entity Framework Core

ASP.NET Core

Blazor
```

Rule:

The Domain must remain framework independent.

---

## Ecommerce.Application

Allowed References:

```text
Ecommerce.Domain

MediatR

FluentValidation

Microsoft.Extensions.Logging.Abstractions
```

Forbidden References:

```text
Ecommerce.Infrastructure

Entity Framework Core DbContext

ASP.NET Controllers

Minimal APIs

Blazor Components
```

Rule:

Application coordinates business use cases.

Application should depend on abstractions only.

---

## Ecommerce.Infrastructure

Allowed References:

```text
Ecommerce.Domain

Ecommerce.Application

Entity Framework Core

Identity

Caching

External Services
```

Forbidden References:

```text
Ecommerce.Api

Ecommerce.Web
```

Rule:

Infrastructure contains implementation details.

Infrastructure should not contain business decisions.

---

## Ecommerce.Api

Allowed References:

```text
Ecommerce.Application

Ecommerce.Infrastructure

Ecommerce.Contracts
```

Forbidden References:

```text
Direct EF Core access

Domain mutations outside Application
```

Rule:

Endpoints remain thin.

Business logic belongs in Application and Domain.

---

## Ecommerce.Web

Allowed References:

```text
Ecommerce.Contracts
```

Preferred:

```text
API calls through typed HTTP clients
```

Forbidden References:

```text
Ecommerce.Infrastructure

Entity Framework Core

Database access

Domain entities
```

Rule:

The UI should communicate through contracts.

The Web project should not know how persistence works.

---

## Ecommerce.Contracts

Allowed References:

```text
.NET Runtime
```

Contains:

```text
Requests

Responses

Contracts

Shared DTOs
```

Forbidden References:

```text
Application

Domain

Infrastructure
```

Rule:

Contracts remain lightweight and transport-oriented.

---

# Namespace Dependencies

Dependencies between namespaces should mirror project dependencies.

Allowed:

```csharp
Ecommerce.Application.Commands
    -> Ecommerce.Domain.Entities
```

Allowed:

```csharp
Ecommerce.Infrastructure.Repositories
    -> Ecommerce.Application.Interfaces
```

Forbidden:

```csharp
Ecommerce.Domain.Entities
    -> Ecommerce.Infrastructure.Persistence
```

Forbidden:

```csharp
Ecommerce.Application.Commands
    -> Ecommerce.Infrastructure.Repositories
```

---

# Domain Rules

The Domain contains business rules.

Allowed:

```text
Entities

Value Objects

Aggregates

Specifications

Repository Interfaces

Domain Events

Business Exceptions
```

Forbidden:

```text
DbContext

HttpContext

ILogger

Configuration

JSON Serialization

API Models

DTOs
```

---

# Application Layer Rules

The Application layer contains use cases.

Allowed:

```text
Commands

Queries

Handlers

Validators

Interfaces

DTOs

Behaviors
```

Forbidden:

```text
SQL

HTTP concerns

UI concerns

Persistence implementations
```

Application must depend on abstractions.

Example:

```csharp
IOrderRepository
```

not

```csharp
OrderRepository
```

---

# Infrastructure Rules

Infrastructure contains implementation details.

Examples:

```text
OrderRepository

ApplicationDbContext

IdentityUserStore

StripePaymentProvider
```

Infrastructure implements interfaces defined elsewhere.

Example:

```csharp
public sealed class OrderRepository
    : IOrderRepository
{
}
```

---

# API Rules

Endpoints should:

```text
Receive Requests

Send Commands

Send Queries

Return Responses
```

Endpoints should not:

```text
Query DbContext directly

Contain business rules

Perform calculations
```

Good:

```csharp
var result = await mediator.Send(command);
```

Bad:

```csharp
var order = new Order();

order.AddItem(...);

await dbContext.SaveChangesAsync();
```

---

# Blazor Rules

Blazor components should contain presentation logic.

Allowed:

```text
Rendering

Validation messages

State management

User interactions
```

Forbidden:

```text
Business rules

Database access

Persistence logic
```

Prefer:

```text
Blazor
    -> API
    -> Application
    -> Domain
```

Avoid:

```text
Blazor
    -> Domain
```

---

# Mapping Rules

Domain entities should never be returned directly from APIs.

Allowed:

```csharp
Product
    -> ProductDto
```

Allowed:

```csharp
Order
    -> OrderResponse
```

Forbidden:

```csharp
return order;
```

from API endpoints.

---

# MediatR Rules

All use cases should be exposed through commands or queries.

Examples:

```text
CreateOrderCommand

CancelOrderCommand

GetOrderByIdQuery
```

Handlers belong in Application.

Endpoints should communicate through:

```csharp
IMediator

ISender
```

---

# Repository Rules

Repository interfaces belong in:

```text
Domain

or

Application
```

Repository implementations belong in:

```text
Infrastructure
```

Preferred:

```csharp
IOrderRepository
OrderRepository
```

Forbidden:

```csharp
GenericRepository<T>
```

unless a documented architectural decision approves it.

---

# Testing Dependencies

## Domain Tests

Allowed:

```text
Domain
```

---

## Application Tests

Allowed:

```text
Application
Domain
```

---

## Infrastructure Tests

Allowed:

```text
Infrastructure
Application
Domain
```

---

## API Tests

Allowed:

```text
Api
Infrastructure
Application
Domain
```

---

# Architecture Enforcement

Architecture tests should verify:

```text
Domain does not reference Infrastructure

Domain does not reference API

Application does not reference Infrastructure

Application does not reference API

Infrastructure does not reference Web

Contracts remain independent
```

Violations should fail the build.

---

# Dependency Injection Rules

Use constructor injection exclusively.

Preferred:

```csharp
public OrderService(
    IOrderRepository repository)
{
}
```

Forbidden:

```csharp
IServiceProvider

Service Locator

Static Container Access
```

---

# Future Expansion Rules

Future bounded contexts should follow the same dependency model.

Examples:

```text
Inventory

Payments

Shipping

Promotions

Reviews
```

New features must not introduce dependency violations.

---

# Architecture Review Checklist

Before merging a pull request verify:

- Dependencies flow inward.
- Domain remains framework independent.
- Business rules remain in Domain.
- Application uses abstractions.
- Infrastructure contains implementations.
- API remains thin.
- Blazor remains presentation-focused.
- Architecture tests pass.

If any dependency rule is violated, refactor before merging.