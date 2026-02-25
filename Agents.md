# SeatHoldApi — Coding Agent Rules

## Project Purpose
This project is a small, clean ASP.NET Core Web API built as a take-home
exercise demonstrating senior-level service design.

Primary goals:
- correctness of business rules
- clean separation of concerns
- testable architecture
- readable, maintainable code

This is NOT a pattern demonstration project.

Avoid introducing unnecessary patterns or abstractions.

---

## Relationship to BRefSamples Workspace
The workspace `../Source` directory is a READ-ONLY reference codebase.

- Learn patterns from Source.
- Do NOT copy large code blocks.
- Simplify patterns for this project.
- Keep implementation minimal and buildable.

---

## Architecture Rules

Preferred structure:

Api → Services → Repository → Domain Models

Rules:
- Controllers remain thin.
- Business logic belongs in services.
- Repository abstracts persistence.
- Domain models contain no framework attributes.

Avoid:
- CQRS
- MediatR
- unnecessary layers
- premature extensibility

---

## Testing Standards

Framework: MSTest

Requirements:
- Service layer must have unit tests.
- No Azure dependencies in tests.
- Prefer in-memory implementations or fakes.

All tests must pass via:

    dotnet test

---

## API Design Rules

- Use UTC time only.
- Do not persist derived state (e.g., IsActive).
- Use ProblemDetails for error responses.
- Prefer explicit readable logic over clever abstractions.

---

## Complexity Control

This is intentionally a small system.

If multiple designs are possible:
choose the simpler one.

Avoid adding features not required by the exercise
except small diagnostic helpers.

---

## Build Verification

After changes:

    dotnet build
    dotnet test

must succeed.
