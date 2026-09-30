# CreateProduct Quick Reference

## File Structure

```
src/Ecommerce.Application/Catalog/
├── Commands/CreateProduct/
│   ├── CreateProductCommand.cs
│   ├── CreateProductCommandHandler.cs
│   └── CreateProductCommandValidator.cs
└── DTOs/
    ├── ProductDto.cs
    └── ProductMappingExtensions.cs
```

---

## End-to-End Flow

```
HTTP Request (POST /api/v1/products)
    ↓
Endpoint receives CreateProductRequest
    ↓
Map Request → CreateProductCommand
    ↓
mediator.Send(command)
    ↓
MediatR Pipeline:
    └─ ValidationBehavior
        └─ CreateProductCommandValidator validates input
           ├─ SKU: not empty, max 100 chars
           ├─ Name: not empty, max 200 chars
           ├─ Price: >= 0, decimal precision
           ├─ Currency: ISO 4217 code
           ├─ CategoryId: not empty
           └─ Description: optional, max 2000 chars
    └─ CreateProductCommandHandler.Handle()
        1. Map command → domain value objects (Sku, Money)
        2. Call Product.Create() [domain factory]
        3. Check domain result
        4. If failure → return Result<Guid>.Failure()
        5. Fetch repository by SKU (check duplicate)
        6. If exists → return Result<Guid>.Failure("SKU_ALREADY_EXISTS")
        7. repository.AddAsync(product) [Infrastructure handles transaction]
        8. Return Result<Guid>.Success(productId)
    ↓
Result<Guid> returned from handler
    ↓
Endpoint unpacks Result:
    ├─ If Success → fetch product via repository
    ├─ product.ToDto() [mapping extension]
    ├─ HTTP 201 Created + Location header
    └─ Response body: ProductDto
    ├─ If Failure → HTTP 400 Bad Request
    └─ Response body: { error, message }
    ↓
HTTP Response (201 or 400)
```

---

## Key Classes

### CreateProductCommand (IRequest<Result<Guid>>)

**File:** `CreateProductCommand.cs`

```csharp
public sealed record CreateProductCommand(
    string Sku,
    string Name,
    decimal Price,
    Guid CategoryId,
    string? Description = null,
    string Currency = "USD") : IRequest<Result<Guid>>;
```

- Immutable record for thread-safety
- Request DTO for MediatR pipeline
- Contains all user inputs for product creation

---

### CreateProductCommandValidator

**File:** `CreateProductCommandValidator.cs`

```csharp
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(cmd => cmd.Sku).NotEmpty().MaximumLength(100);
        RuleFor(cmd => cmd.Name).NotEmpty().MaximumLength(200);
        RuleFor(cmd => cmd.Price).GreaterThanOrEqualTo(0m).PrecisionScale(18, 2);
        RuleFor(cmd => cmd.Currency).NotEmpty().Length(3).Matches("^[A-Z]{3}$");
        RuleFor(cmd => cmd.CategoryId).NotEqual(Guid.Empty);
        RuleFor(cmd => cmd.Description).MaximumLength(2000).When(cmd => !string.IsNullOrWhiteSpace(cmd.Description));
    }
}
```

- Runs before handler via MediatR pipeline
- User-friendly validation error messages
- Prevents invalid data from reaching domain logic

---

### CreateProductCommandHandler (IRequestHandler)

**File:** `CreateProductCommandHandler.cs`

```csharp
public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) { ... }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Map to domain value objects
        var sku = new Sku(request.Sku);
        var price = new Money(request.Price, request.Currency);

        // 2. Call domain factory
        var productResult = Product.Create(sku, request.Name, price, request.CategoryId, request.Description);
        if (!productResult.IsSuccess) return Result<Guid>.Failure(...);

        // 3. Check duplicate SKU
        var existing = await _repository.GetBySkuAsync(sku, cancellationToken);
        if (existing is not null) return Result<Guid>.Failure("SKU_ALREADY_EXISTS", ...);

        // 4. Persist
        await _repository.AddAsync(productResult.Value, cancellationToken);

        // 5. Return success
        return Result<Guid>.Success(productResult.Value.Id);
    }
}
```

- Orchestrates entire use case
- No business logic (delegated to domain)
- Handles errors gracefully
- Depends on IProductRepository abstraction only

---

### ProductDto

**File:** `ProductDto.cs`

```csharp
public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    Guid CategoryId,
    bool IsActive)
{
    public string PriceDisplay => $"{Currency} {Price:F2}";
}
```

- API response contract
- Immutable, serializable
- Computed property for display formatting

---

### ProductMappingExtensions

**File:** `ProductMappingExtensions.cs`

```csharp
public static class ProductMappingExtensions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto(
            Id: product.Id,
            Sku: product.Sku.Value,
            Name: product.Name,
            Description: product.Description,
            Price: product.Price.Amount,
            Currency: product.Price.Currency,
            CategoryId: product.CategoryId,
            IsActive: product.IsActive);
    }
}
```

- Projects domain entity → DTO
- Used in endpoints to shape responses
- Keeps domain details private

---

## Integration Checklist

- [ ] Register MediatR in DependencyInjection
- [ ] Register Validators from assembly
- [ ] Register IProductRepository implementation (Infrastructure)
- [ ] Add ValidationBehavior to MediatR pipeline
- [ ] Create API endpoint to invoke command
- [ ] Add mapping extension to endpoint response
- [ ] Add integration tests for handler
- [ ] Add unit tests for validator
- [ ] Document API endpoint in OpenAPI/Swagger

---

## Example API Endpoint

```csharp
app.MapPost("/api/v1/products", CreateProductAsync)
    .WithName("CreateProduct")
    .Produces<ProductDto>(StatusCodes.Status201Created)
    .ProducesProblem(400)
    .ProducesProblem(500);

async Task<IResult> CreateProductAsync(
    CreateProductRequest request,
    IMediator mediator,
    CancellationToken cancellationToken)
{
    var command = new CreateProductCommand(
        request.Sku,
        request.Name,
        request.Price,
        request.CategoryId,
        request.Description);

    var result = await mediator.Send(command, cancellationToken);

    if (!result.IsSuccess)
    {
        return Results.BadRequest(new { error = result.Error?.Code, message = result.Error?.Message });
    }

    // Get created product and return DTO
    var product = await repository.GetByIdAsync(result.Value, cancellationToken);
    var dto = product.ToDto();

    return Results.Created($"/api/v1/products/{result.Value}", dto);
}
```

---

## Error Codes

| Code | Meaning | HTTP |
|------|---------|------|
| `INVALID_PRODUCT_DATA` | Value object creation failed (Sku, Money) | 400 |
| `SKU_ALREADY_EXISTS` | SKU not unique in catalog | 400 |
| `INVALID_PRICE` | Price validation failed (domain) | 400 |
| `NAME_TOO_LONG` | Name exceeds 200 chars (domain) | 400 |
| `CATEGORY_REQUIRED` | CategoryId is empty (domain) | 400 |
| `OPERATION_CANCELLED` | CancellationToken triggered | 400 |
| `PRODUCT_CREATION_ERROR` | Unexpected error (generic) | 500 |
| FluentValidation errors | Request validation failed | 400 |

---

## Testing

### Unit Test Template

```csharp
[TestClass]
public class CreateProductCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_WithValidCommand_ReturnSuccessWithProductId()
    {
        // Arrange
        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), default))
            .ReturnsAsync((Product?)null);

        var handler = new CreateProductCommandHandler(repositoryMock.Object);
        var command = new CreateProductCommand("SKU-001", "Test Product", 99.99m, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WithDuplicateSku_ReturnFailure()
    {
        // Arrange
        var existing = Product.Create(
            new Sku("SKU-001"),
            "Existing",
            new Money(50m, "USD"),
            Guid.NewGuid()).Value;

        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), default))
            .ReturnsAsync(existing);

        var handler = new CreateProductCommandHandler(repositoryMock.Object);
        var command = new CreateProductCommand("SKU-001", "Different", 99.99m, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("SKU_ALREADY_EXISTS", result.Error?.Code);
    }
}
```

### Validator Test Template

```csharp
[TestClass]
public class CreateProductCommandValidatorTests
{
    [TestMethod]
    public void Validate_WithValidCommand_ReturnsNoErrors()
    {
        // Arrange
        var validator = new CreateProductCommandValidator();
        var command = new CreateProductCommand("SKU-001", "Test Product", 99.99m, Guid.NewGuid());

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_WithEmptySku_ReturnsError()
    {
        // Arrange
        var validator = new CreateProductCommandValidator();
        var command = new CreateProductCommand("", "Test Product", 99.99m, Guid.NewGuid());

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "Sku"));
    }
}
```

---

## Architecture Patterns

| Pattern | Implementation |
|---------|-----------------|
| **CQRS** | CreateProductCommand (Query part in separate Query handlers) |
| **MediatR** | IRequest<Result<Guid>>, IRequestHandler |
| **FluentValidation** | AbstractValidator<CreateProductCommand> |
| **Result<T>** | Explicit error handling without exceptions |
| **Repository Pattern** | IProductRepository abstraction |
| **Value Objects** | Sku, Money enforce invariants |
| **Domain Factory** | Product.Create() static method |
| **DTO** | ProductDto prevents domain leakage |
| **Extension Methods** | ProductMappingExtensions for cleaner code |

---

## Key Principles

1. **Handler is Orchestrator**: Coordinates domain, not executor of logic
2. **Domain Logic is in Domain**: Product.Create() is rich factory method
3. **Validation is Layered**: Request level (FluentValidation) + Domain level (guard clauses)
4. **Error Handling is Explicit**: Result<T> pattern, no exception leakage
5. **Abstractions Protect Domain**: IProductRepository keeps EF Core out of domain
6. **DTOs Shape Responses**: ProductDto isolates internal structure
7. **Immutability for Safety**: Record types, private setters in domain
