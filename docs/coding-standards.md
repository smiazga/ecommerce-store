# Coding Standards

## Purpose

This document defines coding standards, architecture rules, testing conventions, and development practices for the Ecommerce Store solution.

Goals:

- Consistency
- Maintainability
- Readability
- Testability
- Scalability

When in doubt, prefer simplicity and readability over cleverness.

---

# Core Principles

The solution follows:

- SOLID
- Clean Architecture
- Domain Driven Design (DDD)
- CQRS
- Dependency Injection
- Test-Driven Thinking

All code should be:

- Easy to understand
- Easy to test
- Easy to maintain
- Explicit rather than implicit

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

# Architecture Rules

## Dependency Direction

Allowed dependencies:

```text
Domain

Application
    -> Domain

Infrastructure
    -> Application
    -> Domain

Api
    -> Application
    -> Infrastructure
    -> Contracts

Web
    -> Contracts
```

Forbidden:

```text
Domain -> Infrastructure

Domain -> API

Application -> Infrastructure

Application -> API
```

---

# Naming Conventions

## Classes

Use meaningful names.

Good:

```csharp
OrderRepository

CreateOrderCommand

ProductService
```

Avoid:

```csharp
Manager

Helper

Processor

Util
```

---

## Interfaces

Prefix with I.

```csharp
IOrderRepository

IProductService

ICustomerContext
```

---

## Commands

```csharp
CreateOrderCommand

UpdateProductCommand

DeleteProductCommand
```

---

## Queries

```csharp
GetOrderByIdQuery

GetProductsByCategoryQuery

GetCustomerOrdersQuery
```

---

## Validators

```csharp
CreateOrderCommandValidator

UpdateProductCommandValidator
```

---

## DTOs

```csharp
ProductDto

OrderDto

CustomerDto
```

---

## Request Models

```csharp
CreateOrderRequest

CreateProductRequest
```

---

## Response Models

```csharp
CreateOrderResponse

ProductResponse
```

---

# C# Standards

## Nullable Reference Types

Nullable reference types are required.

```xml
<Nullable>enable</Nullable>
```

Avoid:

```csharp
string Name = null!;
```

Prefer:

```csharp
ArgumentNullException.ThrowIfNull(name);
```

---

## Var Usage

Use var when type is obvious.

Good:

```csharp
var order = new Order();
```

Good:

```csharp
var products = await repository.GetAsync();
```

Avoid:

```csharp
var result = customer.CalculateDiscount();
```

if type is not obvious.

---

## Braces

Always use braces.

Good:

```csharp
if (isValid)
{
    Save();
}
```

Never:

```csharp
if (isValid)
    Save();
```

---

## Guard Clauses

Prefer guard clauses.

Good:

```csharp
ArgumentNullException.ThrowIfNull(order);

if (quantity <= 0)
{
    throw new ArgumentOutOfRangeException(
        nameof(quantity));
}
```

---

# Async Standards

All I/O operations must be asynchronous.

Prefer:

```csharp
Task<T>

ValueTask<T>
```

Avoid:

```csharp
.Result

.Wait()
```

Never:

```csharp
async void
```

except UI event handlers.

---

# Dependency Injection

Use constructor injection exclusively.

Good:

```csharp
public OrderService(
    IOrderRepository repository,
    ILogger<OrderService> logger)
{
    _repository = repository;
    _logger = logger;
}
```

Avoid:

```csharp
serviceProvider.GetService<T>();
```

Avoid:

```csharp
ServiceLocator.Get<T>();
```

---

# Domain Standards

## Domain Model

Domain objects should contain behavior.

Good:

```csharp
order.AddItem(product, quantity);

order.Complete();
```

Avoid:

```csharp
order.OrderItems.Add(item);

order.Status = OrderStatus.Completed;
```

---

## Encapsulation

Prefer:

```csharp
private readonly List<OrderItem> _items = [];

public IReadOnlyCollection<OrderItem> Items
    => _items.AsReadOnly();
```

Avoid:

```csharp
public List<OrderItem> Items { get; set; }
```

---

## Value Objects

Use records for value objects.

```csharp
public sealed record Money(
    decimal Amount,
    string Currency);
```

Examples:

- Money
- Address
- EmailAddress
- PhoneNumber

---

## Domain Events

Use domain events when business actions occur.

Examples:

```csharp
OrderPlacedEvent

PaymentCompletedEvent

ProductInventoryLowEvent
```

---

# CQRS Standards

Commands mutate state.

Queries read state.

Never mix responsibilities.

Good:

```csharp
CreateOrderCommand

CreateOrderCommandHandler
```

```csharp
GetOrderByIdQuery

GetOrderByIdQueryHandler
```

---

# Validation Standards

Use FluentValidation.

Validation belongs in validators.

Good:

```csharp
CreateOrderCommandValidator
```

Avoid:

```csharp
if (request.Name == null)
{
}
```

inside handlers.

---

# EF Core Standards

## Configuration

Use Fluent API.

Create one configuration class per entity.

```text
Persistence
 └── Configurations
      └── ProductConfiguration.cs
```

Implement:

```csharp
IEntityTypeConfiguration<T>
```

---

## DbContext

DbContext should remain thin.

Avoid large configuration blocks.

Do not place business logic in DbContext.

---

## Migrations

Store migrations under:

```text
Infrastructure
 └── Persistence
      └── Migrations
```

---

# Repository Standards

Use domain-focused repositories.

Preferred:

```csharp
IOrderRepository

IProductRepository

ICustomerRepository
```

Avoid:

```csharp
IRepository<T>

IGenericRepository<T>
```

Repositories expose business-focused operations.

---

# API Standards

## API Style

Use REST conventions.

Prefer:

```text
GET     /api/products

GET     /api/products/{id}

POST    /api/orders

PUT     /api/products/{id}

DELETE  /api/products/{id}
```

---

## Endpoints

Endpoints should remain thin.

Responsibilities:

- Accept request
- Validate request
- Send command/query
- Return response

Avoid business logic inside endpoints.

---

## Error Handling

Use ProblemDetails.

Avoid exposing internal exception information.

---

# Blazor Standards

Components should have a single responsibility.

Prefer:

```text
Pages/

Components/

Layouts/
```

Avoid large page components containing significant business logic.

Business rules belong in backend services.

---

# Logging Standards

Use:

```csharp
ILogger<T>
```

Prefer structured logging.

Good:

```csharp
_logger.LogInformation(
    "Order {OrderId} created",
    order.Id);
```

Avoid:

```csharp
_logger.LogInformation(
    $"Order {order.Id} created");
```

---

# Testing Standards

## Frameworks

Testing Framework:

- MSTest

Mocking:

- Moq

Assertions:

- FluentAssertions

Architecture Tests:

- NetArchTest

---

## Test Naming

Format:

```text
Method_State_ExpectedResult
```

Examples:

```csharp
AddItem_ValidQuantity_AddsOrderItem

AddItem_InvalidQuantity_ThrowsException

CreateOrder_ValidRequest_ReturnsOrder
```

---

## Test Structure

Follow Arrange / Act / Assert.

```csharp
[TestMethod]
public void AddItem_ValidProduct_AddsItem()
{
    // Arrange

    // Act

    // Assert
}
```

---

## Unit Tests

Focus on:

- Business rules
- Domain behavior
- Validation
- Application logic

Do not test framework code.

---

## Integration Tests

Focus on:

- API endpoints
- EF Core persistence
- Dependency injection configuration
- End-to-end workflows

---

# Architecture Tests

All projects must satisfy architecture boundaries.

Examples:

```csharp
Domain
    must not depend on Infrastructure

Application
    must not depend on API
```

Architecture violations should fail the build.

---

# Security Standards

Never:

- Hardcode secrets
- Store connection strings in source control
- Log sensitive customer data
- Expose internal exception details

Use:

- Configuration Providers
- Secret Manager
- Azure Key Vault

---

# Pull Request Standards

Every PR should:

- Build successfully
- Pass tests
- Follow architecture rules
- Include tests for new behavior
- Include documentation updates when appropriate

Reviewers should verify:

- SOLID compliance
- DDD alignment
- Test coverage
- Security concerns
- Performance concerns

---

# Definition of Done

A feature is complete when:

- Code compiles
- Unit tests pass
- Integration tests pass
- Architecture tests pass
- Documentation is updated
- Code review is completed
- No critical analyzer warnings exist