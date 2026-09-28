# Domain Model

## Purpose

This document defines the business domain for the Ecommerce Store solution.

It serves as the authoritative source for:

- Bounded Contexts
- Aggregates
- Entities
- Value Objects
- Domain Events
- Invariants
- Aggregate Boundaries

All domain implementations must align with this document.

---

# Domain Design Principles

The Domain layer represents business behavior.

Business rules belong in:

- Aggregates
- Entities
- Value Objects

Business rules do not belong in:

- Controllers
- Endpoints
- DbContext
- Repositories
- UI Components

Prefer:

- Rich domain models
- Explicit behaviors
- Protected invariants
- Domain events
- Encapsulation

Avoid:

- Anemic entities
- Public mutable state
- Business logic in handlers
- Business logic in endpoints

---

# SharedKernel Dependencies

Domain objects are built upon SharedKernel abstractions.

Examples:

```text
AggregateRoot

Entity

ValueObject

DomainEvent

DomainException

Result
```

Business concepts belong in Domain.

Framework building blocks belong in SharedKernel.

---

# Bounded Contexts

The system currently consists of four primary bounded contexts.

```text
Catalog

Customers

Cart

Orders
```

Future bounded contexts:

```text
Payments

Inventory

Shipping

Promotions

Reviews
```

---

# Shared Value Objects

The following value objects may be reused across multiple contexts.

---

## Money

Represents a monetary value.

Properties:

```text
Amount

Currency
```

Invariants:

```text
Amount >= 0

Currency required
```

Example:

```csharp
Money usd = new(19.99m, "USD");
```

Used by:

```text
Product

ShoppingCart

Order
```

---

## Address

Represents a shipping or billing address.

Properties:

```text
AddressLine1

AddressLine2

City

State

PostalCode

Country
```

Invariants:

```text
AddressLine1 required

City required

PostalCode required

Country required
```

Used by:

```text
Customer

Order
```

---

## EmailAddress

Represents a validated email.

Properties:

```text
Value
```

Invariants:

```text
Must be valid email format
```

Used by:

```text
Customer
```

---

## Sku

Represents a product identifier.

Properties:

```text
Value
```

Invariants:

```text
Required

Unique
```

Used by:

```text
Product
```

---

# Catalog Context

Responsible for products and categories.

---

# Product Aggregate

Aggregate Root:

```text
Product
```

---

## Responsibilities

Responsible for:

```text
Product information

Pricing

Visibility

Category assignment

Images
```

Not responsible for:

```text
Orders

Inventory management

Shopping carts

Customers
```

---

## Aggregate Structure

```text
Product
│
├── ProductImage
│
└── Category Reference
```

---

## Product Properties

```text
Id

Name

Description

Sku

Price

CategoryId

IsActive
```

---

## Product Invariants

Must always be true:

```text
Name required

Name length <= 200

SKU required

Price >= 0

Category required

Inactive products cannot be purchased
```

---

## Product Behaviors

Preferred:

```csharp
product.Activate();

product.Deactivate();

product.ChangePrice(newPrice);

product.ChangeDescription(description);
```

Avoid:

```csharp
product.IsActive = true;

product.Price = money;
```

---

## ProductImage Entity

Owned by Product.

Properties:

```text
Id

ImageUrl

DisplayOrder
```

Rules:

```text
ImageUrl required

DisplayOrder >= 0
```

---

## Product Domain Events

```text
ProductCreatedDomainEvent

ProductActivatedDomainEvent

ProductDeactivatedDomainEvent

ProductPriceChangedDomainEvent
```

---

# Category Aggregate

Aggregate Root:

```text
Category
```

Responsibilities:

```text
Product categorization

Catalog organization
```

Properties:

```text
Id

Name

Description
```

Invariants:

```text
Name required
```

---

# Customers Context

Responsible for customer information.

---

# Customer Aggregate

Aggregate Root:

```text
Customer
```

---

## Responsibilities

Responsible for:

```text
Identity

Contact Information

Addresses

Account Status
```

Not responsible for:

```text
Orders

Payments

Catalog
```

---

## Aggregate Structure

```text
Customer
│
└── CustomerAddress
```

---

## Customer Properties

```text
Id

FirstName

LastName

EmailAddress

IsActive
```

---

## Customer Invariants

Must always be true:

```text
FirstName required

LastName required

Email required

Email must be unique

Active customer required to place orders
```

---

## Customer Behaviors

Preferred:

```csharp
customer.ChangeEmail(email);

customer.AddAddress(address);

customer.Deactivate();

customer.Activate();
```

---

## CustomerAddress Entity

Owned by Customer.

Properties:

```text
Address

IsDefaultShipping

IsDefaultBilling
```

Rules:

```text
Single default shipping address

Single default billing address
```

---

## Customer Domain Events

```text
CustomerCreatedDomainEvent

CustomerEmailChangedDomainEvent

CustomerActivatedDomainEvent

CustomerDeactivatedDomainEvent
```

---

# Cart Context

Responsible for shopping cart operations.

---

# ShoppingCart Aggregate

Aggregate Root:

```text
ShoppingCart
```

---

## Responsibilities

Responsible for:

```text
Tracking items

Managing quantities

Calculating subtotal
```

---

## Aggregate Structure

```text
ShoppingCart
│
└── ShoppingCartItem
```

---

## ShoppingCart Properties

```text
Id

CustomerId

CreatedDate

UpdatedDate
```

---

## ShoppingCartItem Properties

```text
ProductId

ProductName

UnitPrice

Quantity
```

Snapshot data should be stored.

Product details should not be loaded dynamically from Product during checkout.

---

## ShoppingCart Invariants

Must always be true:

```text
CustomerId required

Quantity > 0

Product may appear only once

Cart may contain multiple items
```

---

## ShoppingCart Behaviors

Preferred:

```csharp
cart.AddItem(product, quantity);

cart.RemoveItem(productId);

cart.UpdateQuantity(productId, quantity);

cart.Clear();
```

---

## Cart Domain Events

```text
ProductAddedToCartDomainEvent

ProductRemovedFromCartDomainEvent

ShoppingCartClearedDomainEvent
```

---

# Orders Context

Responsible for order lifecycle management.

---

# Order Aggregate

Aggregate Root:

```text
Order
```

---

## Responsibilities

Responsible for:

```text
Purchases

Order totals

Order workflow

Order status transitions
```

Order is one of the most important aggregates in the system.

---

## Aggregate Structure

```text
Order
│
└── OrderItem
```

---

## Order Properties

```text
Id

OrderNumber

CustomerId

Status

BillingAddress

ShippingAddress

OrderDate

TotalAmount
```

---

## OrderItem Properties

```text
ProductId

ProductName

UnitPrice

Quantity
```

OrderItem stores a snapshot of product data at the moment of purchase.

---

## Order Status

```text
Pending

Paid

Processing

Shipped

Delivered

Cancelled
```

---

## Order Invariants

Must always be true:

```text
Order must contain at least one item

Order total >= 0

CustomerId required

Billing address required

Shipping address required
```

---

## State Transition Rules

Allowed:

```text
Pending
    -> Paid

Pending
    -> Cancelled

Paid
    -> Processing

Paid
    -> Cancelled

Processing
    -> Shipped

Shipped
    -> Delivered
```

Forbidden:

```text
Delivered
    -> Anything

Cancelled
    -> Anything

Pending
    -> Shipped

Pending
    -> Delivered
```

See:

```text
docs/architecture/diagrams/order-lifecycle.md
```

for the authoritative workflow diagram.

---

## Order Behaviors

Preferred:

```csharp
order.MarkPaid();

order.BeginProcessing();

order.Ship();

order.Deliver();

order.Cancel();
```

Avoid:

```csharp
order.Status = OrderStatus.Shipped;
```

Status changes must always pass through aggregate methods.

---

## Order Domain Events

```text
OrderPlacedDomainEvent

OrderPaidDomainEvent

OrderProcessingStartedDomainEvent

OrderShippedDomainEvent

OrderDeliveredDomainEvent

OrderCancelledDomainEvent
```

---

# Future Contexts

The following contexts are intentionally deferred.

---

## Payments

Future Aggregate:

```text
Payment
```

Potential Statuses:

```text
Pending

Authorized

Captured

Failed

Refunded
```

---

## Inventory

Future Aggregate:

```text
InventoryItem
```

Responsibilities:

```text
Available Quantity

Reserved Quantity

Reorder Levels
```

---

# Aggregate Ownership Rules

Child entities belong to exactly one aggregate.

Examples:

```text
ProductImage
    -> Product

CustomerAddress
    -> Customer

ShoppingCartItem
    -> ShoppingCart

OrderItem
    -> Order
```

Child entities should not be independently loaded.

---

# Aggregate Reference Rules

Aggregates reference each other by identifier.

Preferred:

```csharp
public Guid ProductId { get; }

public Guid CustomerId { get; }
```

Avoid:

```csharp
public Product Product { get; }

public Customer Customer { get; }
```

This preserves aggregate boundaries and transactional consistency.

---

# Domain Event Rules

Events describe things that already happened.

Good:

```text
OrderPlacedDomainEvent

ProductPriceChangedDomainEvent

CustomerDeactivatedDomainEvent
```

Avoid:

```text
PlaceOrderEvent

UpdatePriceEvent
```

Commands express intent.

Events describe completed actions.

---

# Domain Checklist

Before implementing a domain model verify:

- Aggregate root identified.
- Invariants documented.
- Encapsulated behaviors added.
- Child entities owned by aggregate.
- Domain events identified.
- Value objects used where appropriate.
- Aggregate references use IDs.
- Public mutable setters avoided.
- State transitions protected.

If any answer is no, revisit the design before implementation.