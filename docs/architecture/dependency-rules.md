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

Business rules must never depend on frameworks or infrastructure implementations.

The most stable code exists at the center of the architecture.

```text
            Ecommerce.Api
                  │
                  ▼

       Ecommerce.Application
                  │
                  ▼

          Ecommerce.Domain
                  │
                  ▼

      Ecommerce.SharedKernel

                  ▲
                  │

     Ecommerce.Infrastructure


Ecommerce.Web
      │
      ▼

Ecommerce.Contracts
```

The Domain layer is the center of the business architecture.

SharedKernel is the architectural foundation.

Everything may depend on SharedKernel.

The Domain depends only on SharedKernel.

SharedKernel depends on nothing except the .NET runtime.

---

# Project Dependency Diagram

```text
Ecommerce.SharedKernel
        ↑

Ecommerce.Domain
        ↑

Ecommerce.Application
        ↑

Ecommerce.Infrastructure
        ↑

Ecommerce.Api


Ecommerce.Contracts
        ↑

Ecommerce.Web
```

---

# Allowed Project References

## Ecommerce.SharedKernel

Allowed References:

```text
.NET Runtime
```

Contains:

```text
Entity

AggregateRoot

ValueObject

DomainEvent

Result

Result<T>

Guard

DomainException
```

Forbidden:

```text
Product

Order

Customer

ShoppingCart

Repositories

Application Services

Infrastructure Implementations
```

Rule:

SharedKernel contains reusable domain building blocks.

SharedKernel must not contain ecommerce-specific business concepts.

---

## Ecommerce.Domain

Allowed References:

```text
.NET Runtime

Ecommerce.SharedKernel
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

Contains:

```text
Aggregates

Entities

Value Objects

Specifications

Repository Interfaces

Domain Events

Business Rules

Business Exceptions
```

Domain concepts should build upon SharedKernel abstractions.

---

## Ecommerce.Application

Allowed References:

```text
Ecommerce.SharedKernel

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
Ecommerce.SharedKernel

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
Ecommerce.SharedKernel

Ecommerce.Domain

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

SharedKernel
```

Rule:

Contracts remain lightweight and transport-oriented.

---

# SharedKernel Rules

Allowed:

```text
Entity

AggregateRoot

ValueObject

DomainEvent

Result

Result<T>

Guard Clauses

Domain Exceptions
```

Forbidden:

```text
Product

Order

Customer

ShoppingCart

PricingService

OrderRepository

Application Services

Business Workflows
```

A type belongs in SharedKernel only if it can reasonably be reused in a completely different domain.

Examples:

```text
Banking

Healthcare

ERP

Inventory
```

---

# Namespace Dependencies

Dependencies between namespaces should mirror project dependencies.

Allowed:

```csharp
Ecommerce.Domain.Products
    -> Ecommerce.SharedKernel.Common
```

Allowed:

```csharp
Ecommerce.Application.Features.Products
    -> Ecommerce.Domain.Products
```

Allowed:

```csharp
Ecommerce.Infrastructure.Repositories
    -> Ecommerce.Application.Interfaces
```

Forbidden:

```csharp
Ecommerce.Domain.Products
    -> Ecommerce.Infrastructure.Persistence
```

Forbidden:

```csharp
Ecommerce.Application.Features.Products
    -> Ecommerce.Infrastructure.Repositories
```

---

# Aggregate Dependency Rules

Aggregates reference other 