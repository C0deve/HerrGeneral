# HerrGeneral.Primitives

Lightweight, zero-dependency functional primitives and transactional abstractions for **HerrGeneral**.

## Overview

`HerrGeneral.Primitives` defines the core building blocks used across Domain and Application layers without referencing dependency injection, reflection utilities, or execution runtimes.

## Key Types

- **`Result` / `Result<T>`**: Functional result monad supporting success, domain failures, and panic exceptions with expressive `Match` operations.
- **`Unit`**: Representation of a void return value in functional signatures.
- **`IUnitOfWork`**: Lightweight abstraction for managing database transaction boundaries (`Start`, `Commit`, `RollBack`).
- **`DomainException`**: Base exception type for domain validation errors.

## Installation

```bash
dotnet add package HerrGeneral.Primitives
```

## Compatibility

- Targets `.NET 8.0`, `.NET 9.0`, and `.NET 10.0`.
- Zero third-party dependencies.
