# Clean Architecture

## Purpose

This diagram illustrates the architectural layers of the Ecommerce Store application.

Dependencies must flow inward.

Business rules are protected from framework concerns.

---

## Architecture Diagram

```mermaid
flowchart TB

    Web["Blazor Web App"]

    Api["ASP.NET Core Minimal APIs"]

    Application["Application Layer

    Commands
    Queries
    Handlers
    Validators
    DTOs
    Behaviors"]

    Domain["Domain Layer

    Aggregates
    Entities
    Value Objects
    Domain Events
    Specifications"]

    SharedKernel["Shared Kernel

    AggregateRoot
    Entity
    ValueObject
    DomainEvent
    Result"]

    Infrastructure["Infrastructure Layer

    EF Core
    Repositories
    Identity
    Caching
    Payments
    Messaging"]

    Web --> Api

    Api --> Application

    Application --> Domain

    Domain --> SharedKernel

    Infrastructure --> Application
    Infrastructure --> Domain
    Infrastructure --> SharedKernel
```

---

## Layer Responsibilities

### Web

Presentation.

Responsibilities:

- Rendering
- Component composition
- User interactions

---

### API

Application boundary.

Responsibilities:

- Requests
- Responses
- Authentication
- Authorization
- OpenAPI

---

### Application

Use-case orchestration.

Responsibilities:

- CQRS
- MediatR Handlers
- Validation
- DTO Mapping

---

### Domain

Business behavior.

Responsibilities:

- Invariants
- Rules
- State transitions
- Aggregate consistency

---

### SharedKernel

Reusable building blocks.

Responsibilities:

- Entity
- AggregateRoot
- ValueObject
- DomainEvent
- Result

---

### Infrastructure

Technical implementations.

Responsibilities:

- Persistence
- EF Core
- Identity
- External integrations

---