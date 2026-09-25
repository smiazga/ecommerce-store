# Application Patterns

## Purpose

This document defines implementation patterns used throughout the Ecommerce Store solution.

The goal is consistency.

All features should follow these patterns unless an Architecture Decision Record (ADR) explicitly approves an alternative approach.

---

# Application Layer Responsibilities

The Application layer orchestrates use cases.

Responsibilities:

- Commands
- Queries
- Handlers
- Validation
- DTO Mapping
- Transactions
- Authorization Policies
- Business Workflow Coordination

The Application layer does not contain:

- UI logic
- Persistence implementations
- EF Core configuration
- Infrastructure concerns

---

# Feature Organization

Organize features by business capability.

Preferred:

```text
Application

└── Features

    ├── Products
    │   ├── Commands
    │   ├── Queries
    │   ├── DTOs
    │   └── Validators
    │
    ├── Orders
    │   ├── Commands
    │   ├── Queries
    │   ├── DTOs
    │   └── Validators
    │
    └── Customers
        ├── Commands
        ├── Queries
        ├── DTOs
        └── Validators
```

Avoid organizing solely by technical type.

Avoid:

```text
Commands
Queries
Handlers
Validators
```

at the root of the project.

Business-centric organization scales better.

---

# CQRS Pattern

Application follows CQRS.

## Commands

Commands modify state.

Examples:

```text
CreateProductCommand

CreateOrderCommand

UpdateProductCommand

CancelOrderCommand
```

Commands should:

- Express intent
- Be immutable
- Use records

Example:

```csharp
public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price);
```

---

## Queries

Queries retrieve data.

Examples:

```text
GetProductByIdQuery

GetProductsByCategoryQuery

GetCustomerOrdersQuery
```

Queries should not change state.

Example:

```csharp
public sealed record GetProductByIdQuery(
    Guid ProductId);
```

---

# Handler Pattern

Each command or query has one handler.

Examples:

```text
CreateProductCommandHandler

GetProductByIdQueryHandler
```

---

## Command Handler Structure

Preferred:

```csharp
public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        ...
    }
}
```

Handler responsibilities:

- Load aggregates
- Execute domain behavior
- Persist changes
- Return result

Handlers should not:

- Contain validation logic
- Contain HTTP concerns
- Contain UI concerns

---

## Query Handler Structure

Preferred:

```csharp
public sealed class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
}
```

Query handlers should:

- Retrieve data
- Map data
- Return DTOs

Query handlers should not:

- Modify state
- Raise domain events

---

# Validator Pattern

Every command should have a validator.

Preferred:

```text
CreateProductCommandValidator

CreateOrderCommandValidator
```

Example:

```csharp
public sealed class CreateProductCommandValidator
    : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Price)
            .GreaterThan(0);
    }
}
```

Validation belongs here.

Avoid validation in handlers.

---

# Result Pattern

Application should avoid throwing exceptions for expected business outcomes.

Prefer a Result model.

Example:

```csharp
Result

Result<T>
```

---

## Success Example

```csharp
return Result.Success();
```

```csharp
return Result<ProductDto>.Success(product);
```

---

## Failure Example

```csharp
return Result.Failure(
    "Product not found.");
```

---

## Use Exceptions For

Unexpected failures:

```text
Infrastructure failures

Configuration issues

External service failures
```

Do not use exceptions for normal business flow.

---

# DTO Pattern

DTOs belong in Application or Contracts.

Never expose domain entities.

Preferred:

```csharp
public sealed record ProductDto(
    Guid Id,
    string Name,
    decimal Price);
```

Avoid:

```csharp
return Product;
```

from handlers or endpoints.

---

# Mapping Pattern

Use explicit mapping.

Preferred:

```csharp
new ProductDto(
    product.Id,
    product.Name,
    product.Price);
```

Avoid excessive mapping magic.

Mapping should be easy to follow.

---

# Repository Pattern

Repositories represent aggregate boundaries.

Examples:

```csharp
IProductRepository

IOrderRepository

ICustomerRepository
```

Repository interfaces belong in:

```text
Domain

or

Application
```

Implementations belong in:

```text
Infrastructure
```

---

## Repository Example

```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(
        Product product,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}
```

---

# Unit Of Work

EF Core DbContext acts as the Unit Of Work.

Avoid creating custom UnitOfWork abstractions unless a future requirement justifies it.

Preferred:

```csharp
await repository.SaveChangesAsync(
    cancellationToken);
```

---

# Domain Events

Domain events represent business events.

Examples:

```text
OrderPlacedDomainEvent

OrderCancelledDomainEvent

ProductCreatedDomainEvent
```

Events should be raised by aggregates.

Example:

```csharp
AddDomainEvent(
    new OrderPlacedDomainEvent(Id));
```

---

# MediatR Pipeline Behaviors

The following behaviors are recommended.

Order:

```text
Logging

Validation

Performance Monitoring

Transaction

Exception Handling
```

---

## Validation Behavior

Validators execute before handlers.

Invalid requests do not reach handlers.

---

## Logging Behavior

Log:

```text
Command Name

Execution Time

Failures
```

Do not log sensitive customer information.

---

# API Endpoint Pattern

Endpoints remain thin.

Example:

```csharp
app.MapPost(
    "/products",
    async (
        CreateProductCommand command,
        ISender sender,
        CancellationToken cancellationToken) =>
    {
        var result = await sender.Send(
            command,
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    });
```

Endpoints should:

- Bind request
- Dispatch request
- Return response

Nothing more.

---

# CancellationToken Pattern

Pass CancellationToken throughout the request pipeline.

Preferred:

```csharp
await repository.GetByIdAsync(
    id,
    cancellationToken);
```

Never ignore CancellationToken when available.

---

# Logging Pattern

Use structured logging.

Preferred:

```csharp
_logger.LogInformation(
    "Created product {ProductId}",
    product.Id);
```

Avoid:

```csharp
_logger.LogInformation(
    $"Created product {product.Id}");
```

---

# Exception Handling Pattern

Use global exception handling.

Avoid:

```csharp
try
{
}
catch
{
}
```

inside every handler.

Unexpected exceptions should be handled centrally.

Use ProblemDetails in APIs.

---

# Testing Pattern

Every handler should have tests.

---

## Command Test Example

```csharp
[TestMethod]
public async Task Handle_ValidRequest_CreatesProduct()
{
    // Arrange

    // Act

    // Assert
}
```

---

## Query Test Example

```csharp
[TestMethod]
public async Task Handle_ProductExists_ReturnsProduct()
{
    // Arrange

    // Act

    // Assert
}
```

---

# Architecture Checklist

Before implementing a feature verify:

- Command created?
- Query created?
- Validator created?
- Handler created?
- DTO created?
- Repository abstraction used?
- CancellationToken propagated?
- Tests created?
- Domain behavior protected?
- Endpoint remains thin?

If not, refactor before merging.