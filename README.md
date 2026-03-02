# SeatHold API

A small .NET 8 Web API that allows clients to place a temporary hold on a seat and retrieve it later.

This repository is structured as an interview-quality reference implementation that emphasizes:

- Correctness of business rules
- Clean architecture and separation of concerns
- Testability
- Production‑style structure without over‑engineering
- Incremental enhancement across versions

---

## Overview

The API supports:

- Creating a seat hold with expiration
- Retrieving a hold by ID
- Enforcing one active hold per seat (UTC-based expiration)

### Core Rule

> A seat can only have **one active hold at a time**.

A hold is considered **active** when:

```
ExpiresAtUtc > current UTC time
```

---

## Solution Structure

```
SeatHoldApi
│
├── SeatHold.Api          # ASP.NET Core Web API (controllers + middleware)
├── SeatHold.Core         # Domain models, services, interfaces
├── SeatHold.Persistence  # EF Core SQLite persistence (V3)
├── SeatHold.Tests        # Unit + integration tests
└── postman/              # Postman + Newman contract tests (V2)
```

Architecture flow:

```
Controller
   ↓
HoldService
   ↓
IHoldRepository
   ↓
Persistence (InMemory or SQLite)
```

Controllers remain thin; business logic lives in the service layer.

---

## Versions (Branch Strategy)

This repo intentionally evolves across branches.

### Version 1 — Core Implementation (`main`)

Requirements-only solution:

- ASP.NET Core Web API
- In-memory repository
- Service-layer business logic
- ProblemDetails error handling
- Unit tests + integration tests
- Swagger support

Goal: **simple, correct, readable**.

---

### Version 2 — Contract Testing (`feature/postman-newman`)

Adds external API validation:

- Postman collection
- Newman CLI execution
- GitHub Actions automation
- Consumer-level API contract tests

Demonstrates API stability outside the .NET ecosystem.

Run locally:

```bash
pwsh postman/run-newman.ps1
```

---

### Version 3 — SQLite Persistence (`feature/sqlite-persistence`)

Adds relational persistence aligned with typical backend stacks.

- EF Core + SQLite
- Repository implementation swap
- Migrations support
- Transactional insert with re-check to enforce active hold rule
- Database auto-migration on startup

No API or service-layer changes required.

---

## Running the API

```bash
dotnet run --project SeatHold.Api
```

Swagger UI:

```
https://localhost:xxxx/swagger
```

---

## SQLite Mode (Version 3)

Connection string (appsettings.json):

```json
"ConnectionStrings": {
  "SeatHoldDb": "Data Source=seathold.db"
}
```

On startup the app runs:

```
db.Database.Migrate()
```

### Create migrations

```bash
dotnet tool run dotnet-ef migrations add InitialSqlite \
  --project SeatHold.Persistence \
  --startup-project SeatHold.Api
```

### Update database

```bash
dotnet tool run dotnet-ef database update \
  --project SeatHold.Persistence \
  --startup-project SeatHold.Api
```

## Persistence Configuration

V3 defaults to SQLite persistence using EF Core and migrations.

The persistence provider can be switched via configuration:

```json
"Persistence": {
  "Provider": "Sqlite" // or "InMemory"
}
---

## Testing

### Unit + Integration Tests

```bash
dotnet test
```

### Postman / Newman Contract Tests

```bash
pwsh postman/run-newman.ps1
```

CI runs:

- build
- unit tests
- integration tests
- Newman contract tests

---

## Error Handling

API errors return RFC7807 ProblemDetails responses:

- `400` Invalid request
- `404` Not found
- `409` Seat already held

---

## Design Decisions

- UTC timestamps to avoid timezone ambiguity
- Repository abstraction for persistence swap
- Service layer owns business rules
- SQLite chosen for lightweight relational demo
- Deterministic tests with isolated data

---

## Future Enhancements (Not Implemented)

- Distributed locking strategy
- Expired hold cleanup job
- Docker containerization
- Observability / structured logging
- Frontend SPA client

---

## Evolution of the Solution

Project versions demonstrates evolution:

1. **V1:** correctness and clarity
2. **V2:** contract testing maturity
3. **V3:** production persistence upgrade

Each step adds capability without rewriting earlier layers.

---

## License

Interview / demonstration project.
