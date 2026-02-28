Seat Hold API (.NET 8)

A small ASP.NET Core Web API that allows a user to place a temporary hold on a seat and retrieve it later.

This project was implemented as a take-home exercise with emphasis on:

correctness of business rules

clean, maintainable architecture

clear separation of responsibilities

testability and reliability

The goal was to keep the system simple but production-minded, prioritizing clarity over unnecessary features.

Overview

The API allows clients to:

Create a temporary seat hold

Retrieve a hold by its identifier

View current system state via a diagnostic endpoint

A seat may only have one active hold at a time.
A hold is considered active until its expiration time (UTC).

Architecture

The application follows a lightweight layered design:

API → Service → Repository → Domain Model

Design Principles

Controllers act as thin HTTP adapters

Business rules live in the service layer

Persistence is abstracted behind a repository interface

Domain models contain no framework dependencies

Time is injected via ISystemClock for deterministic testing

Derived state (e.g., active/expired) is computed, not stored

This structure keeps the system easy to reason about and straightforward to evolve.

Technology Stack

.NET 8

ASP.NET Core Web API (Controllers)

MSTest

In-memory repository (for simplicity and reproducibility)

Swagger / OpenAPI

Integration testing via WebApplicationFactory

Endpoints
Create Hold

POST /holds

Request body:

{
"seatId": "A12",
"heldBy": "Victor",
"durationMinutes": 15
}

Rules:

All fields are required

durationMinutes must be greater than zero

Only one active hold per seat is allowed

Expiration is calculated using UTC time

Responses:

201 Created — hold created successfully

400 Bad Request — invalid input

409 Conflict — seat already actively held

Get Hold

GET /holds/{id}

Responses:

200 OK — hold found

404 Not Found — hold does not exist

Diagnostic Endpoint (Non-Required)

GET /holds?status=active|expired

Returns all holds, optionally filtered by status.

This endpoint was intentionally added to simplify testing and verification of expiration behavior and is not required by the exercise specification.

Error Handling

Errors are returned using standardized ProblemDetails responses (RFC 7807).

Example:

{
"status": 409,
"title": "Seat already held",
"detail": "Seat 'A12' already has an active hold."
}

Centralized middleware ensures consistent error responses across the API.

Running the API
Requirements

.NET 8 SDK

Run locally

dotnet run --project SeatHold.Api

Open Swagger UI:

https://localhost
:<port>/swagger

Swagger can be used to test all endpoints interactively.

Running Tests

dotnet test

Test coverage includes:

Service layer unit tests (business rules)

End-to-end integration tests (HTTP behavior)

Testing Approach
Unit Tests

Validate business logic independently from ASP.NET:

hold creation rules

expiration logic

concurrency behavior

validation failures

A fake clock (FakeClock) is used to control time deterministically.

Integration Tests

Use WebApplicationFactory to run the API in memory and verify:

HTTP status codes

JSON responses

middleware behavior

routing and dependency injection

Design Decisions
UTC Time

All expiration logic uses UTC to avoid timezone ambiguity.

Derived State

Active/expired status is computed dynamically:

IsActive = ExpiresAtUtc > currentUtcTime

This prevents data inconsistency.

In-Memory Persistence

An in-memory repository was chosen to:

keep setup minimal

ensure reproducible execution

avoid external dependencies

The repository abstraction allows easy replacement with a relational database (e.g., SQLite or SQL Server).

Future Enhancements

Potential extensions if this were a production system:

SQLite or SQL Server persistence

distributed locking strategy

authentication/authorization

pagination for diagnostic endpoint

CI pipeline (GitHub Actions)

simple frontend client

Project Structure

SeatHoldApi
│
├── SeatHold.Api (ASP.NET Core Web API)
├── SeatHold.Core (domain + business logic)
└── SeatHold.Tests (unit + integration tests)

Author Notes

The implementation intentionally favors readability and correctness over feature completeness.
The solution is designed to demonstrate how a small service can be structured in a maintainable and testable way while remaining simple to understand.