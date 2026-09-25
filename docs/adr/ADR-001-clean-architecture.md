# ADR-001 Clean Architecture

Status: Accepted

## Decision

The solution will follow Clean Architecture.

Dependencies point inward.

Domain knows nothing about:

- EF Core
- ASP.NET
- Blazor
- SQL Server

## Consequences

Benefits

- High testability
- Separation of concerns
- Long-term maintainability