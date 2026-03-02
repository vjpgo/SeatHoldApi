# SeatHold API

A .NET 8 Web API that allows clients to place a temporary hold on a seat and retrieve it later.

This implementation prioritizes correctness, clarity, and clean structure over additional features, in line with the original assignment requirements.

---

## Goal

Build a small API that:

- Creates a temporary hold on a seat
- Enforces business rules around active holds
- Allows retrieval of a hold by ID

---

## Technology Stack

- .NET 8
- ASP.NET Core Web API (controllers)
- In-memory repository (default implementation in `main` branch)
- MSTest (unit + integration tests)
- Swagger (OpenAPI)

---

## API Endpoints

### 1) Create Hold

**POST** `/holds`

#### Request Body

```json
{
  "seatId": "A123",
  "heldBy": "victor@example.com",
  "durationMinutes": 10
}
```

#### Rules Enforced

- All fields are required
- `durationMinutes` must be greater than zero
- A seat can only have one *active* hold at a time
- “Active” means `ExpiresAtUtc > DateTime.UtcNow`

#### Responses

- `201 Created` – Hold successfully created
- `400 Bad Request` – Invalid input
- `409 Conflict` – Seat already actively held

---

### 2) Get Hold

**GET** `/holds/{id}`

#### Responses

- `200 OK` – Hold found
- `404 Not Found` – Hold does not exist

---


### 3) Get Holds : Diagnostic Endpoint (extra)

**GET** /holds?status=active|expired

Returns all holds, optionally filtered by status.

This endpoint was added to simplify verification of expiration behavior.

---

## Project Structure

```
SeatHoldApi.sln
│
├── SeatHold.Api
│   ├── Controllers
│   ├── Middleware
│   └── Extensions
│
├── SeatHold.Core
│   ├── Models
│   ├── Services
│   ├── Repositories
│   └── Exceptions
│
└── SeatHold.Tests
    ├── Unit
    └── Integration
```

### Design Notes

- Controllers are thin and delegate to a service layer.
- Business rules are implemented in `HoldService`.
- Persistence is abstracted behind `IHoldRepository`.
- Error handling uses centralized middleware returning `ProblemDetails` responses.
- Time is abstracted via `ISystemClock` to enable deterministic unit testing.

---

## Running the API

From the repository root:

```powershell
dotnet restore
dotnet build
dotnet run --project SeatHold.Api
```

Swagger UI will be available at:

```
https://localhost:{port}/swagger
```

---

## Running Tests

```powershell
dotnet test
```

The test suite includes:

- Unit tests for business rule validation
- Integration tests using `WebApplicationFactory` to validate HTTP behavior

---

## Notes

This `main` branch contains the original in-memory implementation as required by the assignment.

Additional exploratory branches demonstrate optional enhancements (e.g., persistence upgrades and contract testing), while preserving the same API surface and business logic.

---

## Future Enhancements (Not Implemented in main)

- Persistent storage (SQL/EF Core)
- Distributed locking for multi-instance deployments
- Expired hold cleanup background job
- Structured logging and observability
- Containerization
- Authentication and authorization

---

## Summary

This solution focuses on:

- Clear separation of concerns
- Explicit business rule enforcement
- Testability
- Minimal but production-aware structure

It is intentionally simple and aligned strictly with the stated requirements.
