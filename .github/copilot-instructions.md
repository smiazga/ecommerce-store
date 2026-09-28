# Ecommerce Store - Copilot Instructions

## Purpose

You are assisting with development of the Ecommerce Store solution.

This repository follows:

- Clean Architecture
- Domain Driven Design (DDD)
- SOLID Principles
- CQRS
- Dependency Injection
- Entity Framework Core

Always prioritize maintainability, readability, testability, and consistency with existing repository patterns.

---

# Authoritative Documents

Before generating code, follow these repository documents.

## Architecture

- docs/architecture/system-architecture.md
- docs/architecture/dependency-rules.md
- docs/architecture/application-patterns.md
- docs/architecture/domain-model.md

## Standards

- docs/coding-standards.md

## Architecture Decisions

- docs/adr/

If guidance conflicts:

1. ADR documents
2. Architecture documents
3. Coding standards
4. Copilot instructions

The highest priority source wins.

---

# Technology Stack

Use only approved technologies unless explicitly requested otherwise.

## Platform

- .NET 10
- C# Latest Stable Version

## Front End

- Blazor Web App

## API

- ASP.NET Core Minimal APIs

## Data Access

- Entity Framework Core
- SQL Server

## Application Layer

- MediatR
- FluentValidation

## Testing

- MSTest
- Moq
- FluentAssertions
- NetArchTest

Do not introduce alternative frameworks without justification.

---

# Critical Architecture Rules

Always enforce the following:

- Domain must remain framework independent.
- Domain must not reference Infrastructure.
- Domain must not reference API.
- Domain must not reference Blazor.
- Application must not reference Infrastructure implementations.
- Infrastructure implements abstractions defined by Application and Domain.
- API endpoints must remain thin.
- Business rules belong in the Domain.
- Application coordinates use cases.
- Infrastructure contains implementation details.
- Blazor contains presentation concerns only.
- Domain entities must not be returned directly from APIs.
- Dependencies must flow inward.
- Constructor injection is required.
- GenericRepository<T> is not allowed.

---

# Domain Rules

Model the business domain first.

Prefer:

- Aggregates
- Entities
- Value Objects
- Domain Events
- Explicit business behavior

Avoid:

- Anemic domain models
- Public mutable collections
- Public setters when business rules must be enforced

Prefer:

```csharp
order.AddItem(product, quantity);

order.Complete();