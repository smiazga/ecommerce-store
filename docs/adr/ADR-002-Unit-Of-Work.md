# ADR-002

## Decision

EF Core DbContext acts as the Unit of Work.

Repositories do not call SaveChangesAsync().

Application handlers determine transaction boundaries.

## Consequences

Benefits:

- Single transaction per use case
- Aggregate consistency
- Simpler repositories
- Better support for domain events

Tradeoffs:

- Handlers must explicitly commit changes