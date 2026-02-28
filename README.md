# Seat Hold API (.NET 8)

A small ASP.NET Core Web API that allows a user to place a temporary
hold on a seat and retrieve it later.

This project was implemented as a take-home exercise with emphasis on:

-   Correctness of business rules
-   Clean, maintainable architecture
-   Clear separation of responsibilities
-   Testability and reliability

The goal was to keep the system **simple but production-minded**,
prioritizing clarity over unnecessary features.

------------------------------------------------------------------------

## Overview

The API allows clients to:

-   Create a temporary seat hold
-   Retrieve a hold by its identifier
-   (Optional) View holds for diagnostic purposes

A seat may only have **one active hold at a time**.

A hold is considered **active** if:

    ExpiresAtUtc > currentUtcTime

All expiration logic uses **UTC time**.

------------------------------------------------------------------------

## Architecture

The application follows a lightweight layered design:

    API → Service → Repository → Domain Model

### Design Principles

-   Controllers act as thin HTTP adapters
-   Business rules live in the service layer
-   Persistence is abstracted behind a repository interface
-   Domain models contain no framework dependencies
-   Time is injected via `ISystemClock` for deterministic testing
-   Active/expired state is derived, not stored

This structure keeps the system easy to reason about and straightforward
to evolve.

------------------------------------------------------------------------

## Technology Stack

-   .NET 8
-   ASP.NET Core Web API (Controllers)
-   MSTest
-   In-memory repository (for simplicity and reproducibility)
-   Swagger / OpenAPI
-   Integration testing via `WebApplicationFactory`
-   GitHub Actions CI

------------------------------------------------------------------------

## Endpoints

### Create Hold

**POST** `/holds`

Request body:

``` json
{
  "seatId": "A12",
  "heldBy": "Victor",
  "durationMinutes": 15
}
```

Rules:

-   All fields are required
-   `durationMinutes` must be greater than zero
-   Only one active hold per seat is allowed
-   Expiration is calculated using UTC time

Responses:

-   **201 Created** --- hold created successfully
-   **400 Bad Request** --- invalid input
-   **409 Conflict** --- seat already actively held

------------------------------------------------------------------------

### Get Hold

**GET** `/holds/{id}`

Responses:

-   **200 OK** --- hold found
-   **404 Not Found** --- hold does not exist

------------------------------------------------------------------------

### Diagnostic Endpoint (Optional)

**GET** `/holds?status=active|expired`

Returns all holds, optionally filtered by status.

This endpoint was added to simplify verification of expiration behavior.

------------------------------------------------------------------------

## Error Handling

Errors are returned using standardized **ProblemDetails** responses (RFC
7807).

Example:

``` json
{
  "status": 409,
  "title": "Seat already held",
  "detail": "Seat 'A12' already has an active hold."
}
```

Centralized middleware ensures consistent error responses across the
API.

------------------------------------------------------------------------

## Running the API

### Requirements

-   .NET 8 SDK

### Run locally

``` bash
dotnet run --project SeatHold.Api
```

Open Swagger UI:

    https://localhost:<port>/swagger

Swagger can be used to test all endpoints interactively.

------------------------------------------------------------------------

## Running Tests

``` bash
dotnet test
```

Test coverage includes:

-   Service layer unit tests (business rules)
-   End-to-end integration tests (HTTP behavior)

------------------------------------------------------------------------

## Continuous Integration

A GitHub Actions workflow runs automatically on push and pull request:

-   Restore dependencies
-   Build (Release configuration)
-   Run all unit and integration tests

This ensures the solution remains reproducible and validated.

------------------------------------------------------------------------

## Project Structure

    SeatHoldApi
    │
    ├── SeatHold.Api      → ASP.NET Core Web API
    ├── SeatHold.Core     → Domain models + business logic
    └── SeatHold.Tests    → Unit + integration tests

------------------------------------------------------------------------

## Design Notes

### UTC Time

All expiration logic uses UTC to avoid timezone ambiguity.

### Derived State

Active/expired status is computed dynamically instead of stored.

### In-Memory Persistence

An in-memory repository was chosen to:

-   Keep setup minimal
-   Ensure reproducible execution
-   Avoid external dependencies

The repository abstraction allows easy replacement with a relational
database (e.g., SQLite or SQL Server).

------------------------------------------------------------------------

## Future Enhancements

If extended into a production system:

-   SQLite or SQL Server persistence
-   Distributed locking strategy
-   Authentication/authorization
-   Pagination for diagnostic endpoint
-   Frontend client

------------------------------------------------------------------------

## Author Notes

The implementation intentionally favors readability and correctness over
feature completeness.

The solution demonstrates how a small service can be structured in a
maintainable and testable way while remaining simple and easy to
understand.
