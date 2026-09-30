# CreateProduct Feature - Visual Architecture & Diagrams

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                       API Layer                             │
│                  (Minimal API Endpoint)                     │
│  POST /api/v1/products + Request DTO → HTTP 201/400/500   │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│              Application Layer (CQRS)                       │
│                                                             │
│  CreateProductCommand ──┐                                  │
│  (IRequest<Result<G>>)  ├──► CreateProductCommandValidator │
│                         │    (FluentValidation)            │
│  Factory Method         ├──► CreateProductCommandHandler   │
│                         │    (IRequestHandler<, Result>)  │
│  ProductDto ────────────┤                                  │
│  (API Response)         │    Orchestration Flow            │
│                         └────────────────────┐             │
│                                              │             │
│  ┌──────────────────────────────────────────┘             │
│  │                                                         │
│  ├─► 1. Validate command (FluentValidation)              │
│  │                                                         │
│  ├─► 2. Map to value objects (Sku, Money)                │
│  │                                                         │
│  ├─► 3. Call Product.Create() [Domain]                   │
│  │                                                         │
│  ├─► 4. Check domain result                              │
│  │                                                         │
│  ├─► 5. Check SKU duplicate via repo                     │
│  │                                                         │
│  ├─► 6. Persist via repo.AddAsync()                      │
│  │                                                         │
│  └─► 7. Return Result<Guid>                              │
│                                                             │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼ (IProductRepository interface)
┌─────────────────────────────────────────────────────────────┐
│           Infrastructure Layer (EF Core)                    │
│                                                             │
│  ProductRepository : IProductRepository                    │
│  ├─► GetByIdAsync(Guid id)                                │
│  ├─► GetBySkuAsync(Sku sku)                               │
│  ├─► AddAsync(Product product)  ◄─── Handler calls      │
│  └─► UpdateAsync(Product product)                         │
│                                                             │
│  DbContext (SaveChanges)                                   │
│  └─► SQL Server Database                                  │
│                                                             │
└─────────────────────────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│            Domain Layer (Business Logic)                    │
│                                                             │
│  Product Aggregate Root                                    │
│  ├─ Properties: Name, Sku, Price, CategoryId, IsActive   │
│  ├─ Value Objects: Sku, Money                             │
│  └─ Create() factory method                               │
│     ├─ Guard: Validate all inputs                         │
│     ├─ Invariants: Business rules                         │
│     ├─ Events: RaiseDomainEvent()                         │
│     └─ Returns: Result<Product>                           │
│                                                             │
│  ProductCreatedDomainEvent                                │
│  ├─ ProductId                                             │
│  ├─ Sku                                                   │
│  ├─ Name                                                  │
│  ├─ Price                                                 │
│  └─ CategoryId                                            │
│                                                             │
│  IProductRepository (Interface)                           │
│  - Defined in Domain                                      │
│  - Implemented in Infrastructure                          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Request-Response Flow Diagram

```
┌──────────────────┐
│  HTTP POST       │
│  /api/v1/...     │
│  Request Body    │
└────────┬─────────┘
         │
         ▼
┌──────────────────────────────────────┐
│  Endpoint Handler                    │
│  Map Request → Command               │
│  mediator.Send(command)              │
└────────┬─────────────────────────────┘
         │
         ▼
┌──────────────────────────────────────┐
│  MediatR Pipeline                    │
│                                      │
│  ┌──────────────────────────────┐   │
│  │ Logging Behavior (optional)  │   │
│  └────────────┬─────────────────┘   │
│               ▼                      │
│  ┌──────────────────────────────┐   │
│  │ Validation Behavior          │   │
│  │ Rules: FluentValidation      │   │
│  ├──────────────────────────────┤   │
│  │ ✓ Sku: required, max 100ch   │   │
│  │ ✓ Name: required, max 200ch  │   │
│  │ ✓ Price: >= 0, precision 2   │   │
│  │ ✓ Currency: ISO 4217         │   │
│  │ ✓ CategoryId: not empty      │   │
│  │ ✓ Description: optional,2000 │   │
│  │                              │   │
│  │ If fails: Return error       │   │
│  └────────────┬─────────────────┘   │
│               │                      │
│               ▼ (if validation OK)   │
│  ┌──────────────────────────────┐   │
│  │ CreateProductCommandHandler  │   │
│  │ Handle(                      │   │
│  │   Command,                   │   │
│  │   CancellationToken)         │   │
│  └────────────┬─────────────────┘   │
│               ▼                      │
└──────────────────────────────────────┘
         │
         ▼
┌──────────────────────────────────────┐
│  Handler Orchestration               │
│                                      │
│  1. Guard: Validate command ≠ null   │
│     ├─ ✓ Continue if valid           │
│     └─ ✗ Throw if null               │
│                                      │
│  2. Create value objects             │
│     ├─ sku = new Sku(command.sku)   │
│     ├─ price = new Money(...)       │
│     └─ May throw ArgumentException  │
│                                      │
│  3. Call domain factory              │
│     ├─ Product.Create(...)          │
│     └─ Returns Result<Product>      │
│                                      │
│  4. Check domain result              │
│     ├─ ✓ Success: continue          │
│     └─ ✗ Failure: return error      │
│                                      │
│  5. Repository: Check duplicate SKU  │
│     ├─ repo.GetBySkuAsync(sku)      │
│     ├─ ✓ Not found: continue        │
│     └─ ✗ Found: return error        │
│                                      │
│  6. Persist to database              │
│     ├─ repo.AddAsync(product)       │
│     ├─ DbContext tracks changes     │
│     └─ Transaction: Infrastructure  │
│                                      │
│  7. Return success result            │
│     └─ Result<Guid>.Success(id)     │
│                                      │
└──────────────────┬───────────────────┘
                   │
                   ▼
┌──────────────────────────────────────┐
│  Endpoint Response Handling          │
│                                      │
│  if (result.IsSuccess)               │
│  │  ├─ product = repo.Get(id)       │
│  │  ├─ dto = product.ToDto()        │
│  │  └─ HTTP 201 Created + body      │
│  else                                │
│  │  ├─ code = result.Error.Code     │
│  │  ├─ msg = result.Error.Message   │
│  │  └─ HTTP 400 Bad Request         │
│                                      │
└──────────────────┬───────────────────┘
                   │
                   ▼
┌──────────────────────────────────────┐
│  HTTP Response                       │
│                                      │
│  Success:                            │
│  ├─ Status: 201 Created              │
│  ├─ Location: /api/v1/products/{id}  │
│  └─ Body: ProductDto JSON            │
│                                      │
│  Validation Error:                   │
│  ├─ Status: 400 Bad Request          │
│  └─ Body: { error, message }         │
│                                      │
│  Unexpected Error:                   │
│  ├─ Status: 500 Internal Server      │
│  └─ Body: { error, message }         │
│                                      │
└──────────────────────────────────────┘
```

---

## Class Hierarchy & Dependencies

```
CreateProductCommand (record)
├─ Implements IRequest<Result<Guid>>
├─ Properties: Sku, Name, Price, CategoryId, Description?, Currency="USD"
└─ Used by: MediatR pipeline, CreateProductCommandValidator, Handler

CreateProductCommandValidator
├─ Extends AbstractValidator<CreateProductCommand>
├─ Rules: FluentValidation syntax
└─ Invoked by: MediatR ValidationBehavior

CreateProductCommandHandler
├─ Implements IRequestHandler<CreateProductCommand, Result<Guid>>
├─ Depends on: IProductRepository (injected)
├─ Calls: Product.Create() (domain factory)
└─ Returns: Result<Guid>

ProductDto (record)
├─ Properties: Id, Sku, Name, Description?, Price, Currency, CategoryId, IsActive
├─ Computed: PriceDisplay (getter-only)
└─ Used by: API endpoints, responses

ProductMappingExtensions
├─ Extension method: ToDto(this Product)
├─ Maps: Product aggregate → ProductDto
└─ Used by: Endpoints for response shaping

Product Aggregate Root (Domain)
├─ Is: AggregateRoot (inherits from Entity)
├─ Factory: static Result<Product> Create(...)
├─ Value Objects: Sku, Money
├─ Events: ProductCreatedDomainEvent, ProductActivatedDomainEvent, etc.
└─ Repository: IProductRepository (abstraction)

IProductRepository (Domain Interface)
├─ Method: GetByIdAsync(Guid id)
├─ Method: GetBySkuAsync(Sku sku)
├─ Method: AddAsync(Product product)
├─ Method: UpdateAsync(Product product)
└─ Implemented by: ProductRepository (Infrastructure)
```

---

## Error Handling Flow

```
Handler.Handle()
│
├─ ArgumentException (null check)
│  └─ Caught → INVALID_PRODUCT_DATA
│
├─ Sku value object construction
│  ├─ ArgumentException (empty/null)
│  │  └─ Caught → INVALID_PRODUCT_DATA
│  └─ Guard validates SKU format
│
├─ Money value object construction
│  ├─ ArgumentException (negative amount)
│  │  └─ Caught → INVALID_PRODUCT_DATA
│  └─ Guard validates currency
│
├─ Product.Create() domain factory
│  ├─ NAME_TOO_LONG (name > 200 chars)
│  ├─ INVALID_PRICE (price < 0)
│  ├─ CATEGORY_REQUIRED (categoryId empty)
│  └─ Other domain validations
│      └─ All return Result<Product>.Failure(code, message)
│
├─ Repository.GetBySkuAsync()
│  ├─ Returns: Product? (null if not found)
│  └─ Throws: Exception (DB error, timeout)
│     └─ Caught → PRODUCT_CREATION_ERROR
│
├─ SKU Duplicate Check
│  ├─ if (existingProduct is not null)
│  │  └─ Return: SKU_ALREADY_EXISTS
│  └─ Continue: to persistence
│
├─ Repository.AddAsync()
│  ├─ Throws: DbUpdateException (constraint, etc)
│  └─ Throws: Exception (general DB error)
│     └─ Caught → PRODUCT_CREATION_ERROR
│
└─ OperationCanceledException
   ├─ From GetBySkuAsync()
   ├─ From AddAsync()
   └─ Caught → OPERATION_CANCELLED
```

---

## Test Coverage Map

```
Handler Tests (23 tests)
│
├─ Success Path (6 tests)
│  ├─ Valid with all fields
│  ├─ Valid with minimal fields
│  ├─ Valid without description
│  ├─ Valid with default currency
│  ├─ Valid with zero price
│  └─ Repository call ordering
│
├─ Duplicate Handling (3 tests)
│  ├─ Duplicate SKU detection
│  ├─ No persistence on duplicate
│  └─ CancellationToken propagation
│
├─ Domain Validation (6 tests)
│  ├─ Invalid SKU format
│  ├─ Negative price
│  ├─ Invalid currency
│  ├─ Empty name
│  ├─ Name > 200 chars
│  └─ Empty CategoryId
│
├─ Repository Failures (2 tests)
│  ├─ GetBySkuAsync throws
│  └─ AddAsync throws
│
├─ Cancellation (2 tests)
│  ├─ Cancelled during GetBySkuAsync
│  └─ Cancelled during AddAsync
│
└─ Guard Clauses (1 test)
   └─ Null repository in constructor


Validator Tests (28 tests)
│
├─ Valid Commands (8 tests)
│  ├─ Full command
│  ├─ Minimal required
│  ├─ Zero price
│  ├─ Max length SKU
│  ├─ Max length name
│  ├─ Null description
│  ├─ Empty description
│  └─ Whitespace description
│
├─ SKU Validation (3 tests)
│  ├─ Empty SKU
│  ├─ Whitespace SKU
│  └─ SKU > 100 chars
│
├─ Name Validation (3 tests)
│  ├─ Empty name
│  ├─ Whitespace name
│  └─ Name > 200 chars
│
├─ Price Validation (5 tests)
│  ├─ Negative price
│  ├─ > 2 decimal places
│  ├─ Exactly 2 decimals
│  ├─ 1 decimal place
│  └─ Whole number
│
├─ Currency Validation (8 tests)
│  ├─ Empty currency
│  ├─ Length < 3
│  ├─ Length > 3
│  ├─ Valid currencies
│  ├─ Lowercase
│  ├─ Mixed case
│  └─ Special characters
│
├─ CategoryId Validation (2 tests)
│  ├─ Empty GUID
│  └─ Valid GUID
│
├─ Description Validation (2 tests)
│  ├─ > 2000 chars
│  └─ Max 2000 chars
│
└─ Multiple Errors (1 test)
   └─ Multiple fields invalid
```

---

## Data Model

```
CreateProductCommand
├─ Sku: string
├─ Name: string
├─ Price: decimal
├─ CategoryId: Guid
├─ Description: string? = null
└─ Currency: string = "USD"

Product (Domain Entity)
├─ Id: Guid (from Entity)
├─ Name: string
├─ Description: string?
├─ Sku: Sku (value object)
├─ Price: Money (value object)
├─ CategoryId: Guid
├─ IsActive: bool = false
├─ RowVersion: byte[]? (concurrency)
├─ Images: IReadOnlyCollection<ProductImage>
└─ DomainEvents: IReadOnlyCollection<IDomainEvent>

Sku (Value Object)
└─ Value: string

Money (Value Object)
├─ Amount: decimal (rounded to 2 places)
└─ Currency: string (uppercase)

ProductDto
├─ Id: Guid
├─ Sku: string
├─ Name: string
├─ Description: string?
├─ Price: decimal
├─ Currency: string
├─ CategoryId: Guid
└─ IsActive: bool

Result<Guid>
├─ IsSuccess: bool
├─ Value: Guid (if success)
└─ Error: Error? (if failure)
   ├─ Code: string
   └─ Message: string
```

---

## Dependency Injection Flow

```
Program.cs (Startup)
│
├─ AddMediatR()
│  └─ Scans assembly for:
│     ├─ IRequest<T> implementations
│     ├─ IRequestHandler<TRequest, TResponse>
│     └─ Auto-registers CreateProductCommandHandler
│
├─ AddValidatorsFromAssembly()
│  └─ Scans assembly for:
│     └─ AbstractValidator<T> implementations
│        └─ Registers CreateProductCommandValidator
│
├─ AddPipelineBehavior<ValidationBehavior>()
│  └─ Runs before each handler:
│     ├─ Resolves validator
│     ├─ Calls Validate()
│     └─ Returns failure if invalid
│
├─ AddScoped<IProductRepository>()
│  └─ Resolves to ProductRepository (Infrastructure)
│     └─ Injected into CreateProductCommandHandler
│
└─ [Endpoint]
   └─ Resolves:
      ├─ IMediator (from DI container)
      ├─ IProductRepository (from DI container)
      └─ Both available for use
```

---

## MediatR Pipeline Diagram

```
Request (CreateProductCommand)
    │
    ▼ [Pipeline Start]

┌───────────────────────────────────────┐
│ LoggingBehavior (Pre)                 │
│ Log request details                   │
└────────────┬────────────────────────────┘
             │
             ▼
┌───────────────────────────────────────┐
│ ValidationBehavior (Pre)              │
│ Resolve validator                     │
│ Call validator.Validate(request)      │
│ If invalid: return Result.Failure()   │
│ If valid: continue                    │
└────────────┬────────────────────────────┘
             │
             ▼
┌───────────────────────────────────────┐
│ CreateProductCommandHandler           │
│ .Handle(request, cancellationToken)  │
│ Orchestrate use case                  │
│ Return Result<Guid>                   │
└────────────┬────────────────────────────┘
             │
             ▼
┌───────────────────────────────────────┐
│ LoggingBehavior (Post)                │
│ Log response/result                   │
└────────────┬────────────────────────────┘
             │
             ▼ [Pipeline End]

Result (Result<Guid>)
    └─ Returned to endpoint
       └─ Mapped to HTTP response
```

---

## Summary

This visual architecture demonstrates:

✅ **Layered Architecture** - Clear separation of concerns  
✅ **CQRS Pattern** - Command with explicit handler  
✅ **Repository Pattern** - Abstraction for data access  
✅ **Dependency Injection** - Runtime behavior configuration  
✅ **Error Handling** - Graceful failure with Result<T>  
✅ **Async/Await** - Non-blocking I/O throughout  
✅ **Testing Coverage** - Multiple test scenarios  
✅ **Domain-Driven Design** - Business logic in domain  
