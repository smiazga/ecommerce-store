# CreateProduct Feature - Implementation Guide

## Overview

The CreateProduct feature is a complete CQRS implementation demonstrating best practices for the Ecommerce Store solution. It follows Clean Architecture, Domain-Driven Design, and SOLID principles.

---

## Files Generated

```
src/Ecommerce.Application/
└── Catalog/
    ├── Commands/
    │   └── CreateProduct/
    │       ├── CreateProductCommand.cs          # CQRS Command DTO
    │       ├── CreateProductCommandHandler.cs   # Orchestrator & Coordinator
    │       └── CreateProductCommandValidator.cs # FluentValidation Rules
    └── DTOs/
        ├── ProductDto.cs                        # API Response DTO
        └── ProductMappingExtensions.cs          # Domain → DTO Mapper
```

---

## Component Details

### 1. CreateProductCommand

**Type:** `IRequest<Result<Guid>>` (MediatR)

**Responsibility:** Encapsulates user intent to create a product.

**Key Design Decisions:**
- Uses `record` type for immutability and structural equality
- Implements `IRequest<Result<Guid>>` for MediatR pipeline
- Includes all required input: SKU, Name, Price, CategoryId, Description (optional), Currency (default USD)
- No validation logic—pure data contract
- Serializable for APIs and logging

**Example Usage:**
```csharp
var command = new CreateProductCommand(
    sku: "PROD-001",
    name: "Laptop",
    price: 999.99m,
    categoryId: Guid.Parse("..."),
    description: "High-performance laptop",
    currency: "USD");

var result = await mediator.Send(command, cancellationToken);
```

---

### 2. CreateProductCommandValidator

**Type:** `AbstractValidator<CreateProductCommand>` (FluentValidation)

**Responsibility:** Validates request constraints before handler execution.

**Validation Rules:**
- **SKU**: Required, max 100 chars
- **Name**: Required, max 200 chars
- **Price**: >= 0, max 2 decimal places
- **Currency**: Exactly 3 uppercase letters (ISO 4217)
- **CategoryId**: Not empty GUID
- **Description**: Optional, max 2000 chars when provided

**Key Design Decisions:**
- Stateless, reusable validator
- Runs in MediatR pipeline before handler
- Provides user-friendly error messages
- Separate from domain validation (domain validator is richer)
- Conditional validation using `.When()` for optional fields

**Integration:**
```csharp
// Automatically invoked by MediatR pipeline:
// Pipeline → Validation → Handler
services.AddMediatR(config =>
    config.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));
```

---

### 3. CreateProductCommandHandler

**Type:** `IRequestHandler<CreateProductCommand, Result<Guid>>` (MediatR)

**Responsibility:** Orchestrates the entire use case—validation coordination, domain logic invocation, persistence, error handling.

**Core Flow:**

```
1. Guard clause validation (null checks)
   ↓
2. Map command inputs → domain value objects (Sku, Money)
   ↓
3. Call Product.Create() domain factory
   ↓
4. Handle domain result (success or failure)
   ↓
5. Check for duplicate SKU (business constraint)
   ↓
6. Persist via IProductRepository.AddAsync()
   ↓
7. Return Result<Guid>.Success(productId)
```

**Key Design Decisions:**

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Value Object Mapping** | Done in handler | Domain factory validates value objects; prevents invalid state |
| **Domain Invocation** | `Product.Create()` static factory | Rich domain logic encapsulated; handler coordinates only |
| **SKU Uniqueness** | Checked before persist | Business invariant; prevents duplicates |
| **Transactions** | NOT managed in handler | Infrastructure (DbContext) handles transaction boundaries |
| **Exceptions** | Caught and converted to Result | Explicit error handling; no exception leakage |
| **Error Codes** | Domain-first, then app-level | `INVALID_PRICE` from domain; `SKU_ALREADY_EXISTS` from app |

**Exception Handling:**

- `ArgumentException` → `INVALID_PRODUCT_DATA`
- `OperationCanceledException` → `OPERATION_CANCELLED`
- Unexpected exceptions → `PRODUCT_CREATION_ERROR` (generic for security)

**Dependencies Injected:**
- `IProductRepository`: Domain abstraction—no EF Core direct access

---

### 4. ProductDto

**Type:** Data Transfer Object (DTO)

**Responsibility:** Represents product data for external consumption (APIs, clients).

**Properties:**
- `Id`, `Sku`, `Name`, `Description`, `Price`, `Currency`, `CategoryId`, `IsActive`
- `PriceDisplay`: Computed property for formatted price (e.g., "USD 999.99")

**Key Design Decisions:**
- Uses `record` type (immutable, serializable)
- No domain logic—pure data
- Keeps domain aggregate details private
- **NOT returned directly from handler** (handlers return `Result<Guid>`)
- Used by Mapping layer (Application) to project `Product` → `ProductDto`

**Mapping Example:**
```csharp
// In a mapper or handler continuation:
var productDto = new ProductDto(
    id: product.Id,
    sku: product.Sku.Value,
    name: product.Name,
    description: product.Description,
    price: product.Price.Amount,
    currency: product.Price.Currency,
    categoryId: product.CategoryId,
    isActive: product.IsActive);

// Or using the extension method:
var productDto = product.ToDto();
```

---

## Architecture Compliance

### ✅ Dependency Rules

- **Application** → Domain ✓
- **Application** → SharedKernel ✓
- **Application** ↛ Infrastructure (no direct reference) ✓
- **Application** ↛ API/Web ✓

### ✅ Domain Preservation

- Domain `Product` remains framework-independent ✓
- No EF Core attributes on domain entities ✓
- Repository interface defined in Domain ✓
- Business logic in `Product.Create()`, not handler ✓

### ✅ CQRS Patterns

- Command separates read/write intent ✓
- Handler is orchestrator, not processor ✓
- Validator as separate concern ✓
- DTO prevents domain leakage ✓

### ✅ SOLID Principles

- **S**ingle Responsibility: Each class has one reason to change
- **O**pen/Closed: Extensible via inheritance, closed for modification
- **L**iskov Substitution: `IProductRepository` contract is replaceable
- **I**nterface Segregation: Interfaces (IProductRepository, IRequest, IRequestHandler) are focused
- **D**ependency Inversion: Depends on abstractions (IProductRepository), not implementations

---

## Integration with MediatR Pipeline

### Registration

```csharp
// In DependencyInjection setup (Api project):

services.AddMediatR(config =>
    config.RegisterServicesFromAssembly(
        typeof(CreateProductCommand).Assembly));

services.AddValidatorsFromAssembly(
    typeof(CreateProductCommand).Assembly,
    lifetime: ServiceLifetime.Transient);

// Validation pipeline behavior (built-in or custom)
services.AddTransient(typeof(IPipelineBehavior<,>), 
    typeof(ValidationBehavior<,>));
```

### Execution Flow

```
Request (HTTP POST /api/products)
    ↓
API Endpoint (thin layer)
    ↓
MediatR.Send(CreateProductCommand)
    ↓
[MediatR Pipeline]
    ├─ LoggingBehavior (optional)
    ├─ ValidationBehavior (runs CreateProductCommandValidator)
    │   └─ Returns Result.Failure if invalid
    ├─ CreateProductCommandHandler
    │   ├─ Domain logic invocation
    │   ├─ Repository persistence
    │   └─ Returns Result<Guid>
    └─ [Exception wrapper] (optional)
    ↓
API Endpoint maps Result → HTTP Response (201, 400, 500)
    ↓
Response (HTTP 201 Created + Location header)
```

---

## Example Usage in API Endpoint

```csharp
// In Ecommerce.Api/Endpoints/Products/CreateProductEndpoint.cs

app.MapPost("/api/v1/products", CreateProductAsync)
    .WithName("CreateProduct")
    .ProducesProblem(400)
    .ProducesProblem(500)
    .Produces<ProductDto>(StatusCodes.Status201Created)
    .WithOpenApi();

async Task<IResult> CreateProductAsync(
    CreateProductRequest request,  // API contract (optional)
    IMediator mediator,
    CancellationToken cancellationToken)
{
    // Map API request to command (if using request DTO)
    var command = new CreateProductCommand(
        request.Sku,
        request.Name,
        request.Price,
        request.CategoryId,
        request.Description);

    // Invoke CQRS command
    var result = await mediator.Send(command, cancellationToken);

    // Handle result
    if (!result.IsSuccess)
    {
        return Results.BadRequest(new
        {
            error = result.Error?.Code,
            message = result.Error?.Message
        });
    }

    // Fetch created product and return DTO
    var productId = result.Value;
    var product = await repository.GetByIdAsync(productId, cancellationToken);

    var dto = new ProductDto(
        product.Id,
        product.Sku.Value,
        product.Name,
        product.Description,
        product.Price.Amount,
        product.Price.Currency,
        product.CategoryId,
        product.IsActive,
        product.CreatedAt);

    return Results.Created($"/api/v1/products/{productId}", dto);
}
```

---

## Error Scenarios & Handling

| Scenario | Error Code | Status Code | Message |
|----------|-----------|-----------|---------|
| Invalid SKU format | `INVALID_PRODUCT_DATA` | 400 | ArgumentException message |
| SKU already exists | `SKU_ALREADY_EXISTS` | 400 | "A product with this SKU already exists..." |
| Price < 0 | `INVALID_PRICE` | 400 | "Price must be greater than or equal to zero." |
| Name too long (domain) | `NAME_TOO_LONG` | 400 | "Name must be 200 characters or fewer." |
| Invalid category | `CATEGORY_REQUIRED` | 400 | "Category is required." |
| Request validation fails | (Validation codes) | 400 | FluentValidation errors |
| Operation cancelled | `OPERATION_CANCELLED` | 400 | "Product creation operation was cancelled." |
| Unexpected error | `PRODUCT_CREATION_ERROR` | 500 | Generic message (never expose internals) |

---

## Testing Strategy

### Unit Tests (CreateProductCommandHandlerTests)

```csharp
[TestClass]
public class CreateProductCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_WithValidCommand_ReturnsProductId()
    {
        // Arrange
        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), default))
            .ReturnsAsync((Product?)null);

        var handler = new CreateProductCommandHandler(repositoryMock.Object);
        var command = new CreateProductCommand("PROD-001", "Test", 99.99m, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WithDuplicateSku_ReturnsFailed()
    {
        // Arrange
        var existingProduct = Product.Create(...).Value;
        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), default))
            .ReturnsAsync(existingProduct);

        var handler = new CreateProductCommandHandler(repositoryMock.Object);
        var command = new CreateProductCommand("PROD-001", "Test", 99.99m, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("SKU_ALREADY_EXISTS", result.Error?.Code);
    }
}
```

### Integration Tests

- Test with real `IProductRepository` (EF Core)
- Verify transaction boundaries
- Check domain events are raised
- Validate database state

---

## Key Takeaways

1. **Handler is an Orchestrator**: Coordinates domain, validation, and persistence—doesn't contain business logic
2. **Domain Factory is Rich**: `Product.Create()` encapsulates all creation logic and invariants
3. **Result<T> Pattern**: Eliminates exception-based control flow; explicit success/failure
4. **Repository Abstraction**: No EF Core leakage; handler uses `IProductRepository` interface
5. **DTO Separation**: Domain entities never exposed directly to API consumers
6. **Validation Pipeline**: FluentValidation runs before handler; independent concern
7. **Transaction Boundaries**: Managed by Infrastructure (DbContext), not orchestrator
8. **Error Codes**: Semantic, domain-aware codes for API clients to handle programmatically
