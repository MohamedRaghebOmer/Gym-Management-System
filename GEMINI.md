# General Agent Instructions

You are working on the GYM Management System, a production-oriented application built with C# 14, .NET 10, and Entity Framework Core.

Your goal is to produce clean, maintainable, secure, testable, and production-quality code while strictly respecting the existing architecture and conventions of the repository.

## 1. Understand Before Changing

Before implementing or modifying anything:

1. Inspect the relevant parts of the existing codebase.
2. Understand the existing architecture, patterns, naming conventions, and abstractions.
3. Search for existing implementations that solve a similar problem.
4. Reuse existing abstractions and patterns when appropriate.
5. Do not assume that a class, service, repository, extension, or utility does not exist before checking the repository.
6. Do not introduce a new pattern or abstraction when an existing project pattern already solves the problem.
7. Do not modify unrelated code.

The existing codebase is the primary source of truth for project-specific behavior.

## 2. Architecture

The project follows Clean Architecture.

Respect the dependency direction between layers:

- Domain must remain independent from Application, Infrastructure, and API.
- Application must not depend on Infrastructure or API.
- Infrastructure implements abstractions defined by Application and/or Domain where appropriate.
- API is responsible for HTTP/API concerns and should remain thin.

Do not bypass the existing architecture for convenience.

Business rules must live in the appropriate application/domain layer rather than being placed in controllers, endpoints, repositories, or infrastructure-specific code.

## 3. Existing Conventions

Always follow the conventions already established in the repository for:

- Naming
- Folder structure
- Class organization
- Dependency injection
- DTOs
- Error handling
- Validation
- Logging
- Entity configurations
- Repository patterns
- Unit of Work
- Result/error patterns
- Authentication and authorization
- Testing

When existing code and generic best practices differ, first determine whether the difference is intentional and consistent with the project before changing it.

Do not introduce personal preferences merely for the sake of style.

## 4. Implementation Principles

Prefer:

- Simple solutions over unnecessarily complex ones.
- Explicit and readable code over clever code.
- Small, focused classes and methods.
- Existing abstractions over duplicate abstractions.
- Composition over unnecessary inheritance.
- Immutability where appropriate.
- Strong typing over primitive strings/integers when the domain benefits from it.
- Guard clauses where they improve readability.
- Async APIs for I/O-bound operations.
- CancellationToken propagation for asynchronous operations where applicable.

Avoid:

- Over-engineering.
- Premature abstractions.
- Duplicate logic.
- Dead code.
- Unnecessary comments.
- Unnecessary LINQ complexity.
- Magic strings and magic numbers when a meaningful abstraction is appropriate.
- Blocking asynchronous operations.
- Unnecessary database calls.
- Loading data into memory when the operation can be performed efficiently by the database.

## 5. Modern C# and .NET

Use modern C# 14 and .NET 10 features when they improve readability, correctness, maintainability, or performance.

Do not use a newer language feature merely because it exists.

Follow nullable reference type semantics correctly.

Prefer type-safe APIs and meaningful domain models over loosely typed solutions.

Use dependency injection consistently with the existing project configuration.

## 6. Entity Framework Core

When working with EF Core:

- Follow the existing DbContext and configuration structure.
- Keep entity configurations separate when that is the established project convention.
- Do not put EF Core-specific concerns inside Domain entities.
- Prefer database-side operations when appropriate.
- Avoid unnecessary materialization with `ToList`, `ToArray`, etc.
- Use `AsNoTracking` for read-only queries when appropriate.
- Propagate `CancellationToken` to asynchronous EF Core operations.
- Be aware of query translation limitations.
- Do not assume LINQ expressions will execute in memory; verify that the expression can be translated when necessary.
- Avoid N+1 query patterns.
- Respect existing transaction and Unit of Work conventions.
- Do not introduce raw SQL when EF Core can perform the operation appropriately unless there is a justified reason.

## 7. Security

Treat security as a first-class concern.

When implementing features, consider:

- Authentication
- Authorization
- Resource ownership
- Tenant/Gym isolation
- Input validation
- Sensitive data exposure
- SQL injection
- Improper access control
- Mass assignment
- Logging of sensitive information

Never bypass authorization simply because an endpoint or service is internal.

For this project, data belonging to one Gym must not be accessible or modifiable by users from another Gym unless the existing business rules explicitly allow it.

## 8. Error Handling

Follow the project's existing error-handling and Result/ProblemDetails conventions.

Do not introduce a new error-handling mechanism for a single feature.

Do not silently swallow exceptions.

Catch exceptions only when there is a meaningful reason to handle them at that level.

Do not use exceptions as normal control flow when the existing application patterns provide a better mechanism.

## 9. Performance

Consider performance without sacrificing maintainability.

Pay particular attention to:

- Database round trips
- EF Core query translation
- Unnecessary allocations
- Large object materialization
- N+1 queries
- Repeated database queries
- Unnecessary serialization
- Inefficient collection operations

Do not optimize code based on assumptions. Prefer clear code unless there is a meaningful performance concern.

## 10. Changes and Scope

Keep changes focused on the requested task.

Do not:

- Rename unrelated classes.
- Reformat unrelated files.
- Refactor unrelated code.
- Upgrade packages.
- Change project architecture.
- Change database behavior outside the feature.
- Add dependencies.

unless the requested feature genuinely requires it.

If an architectural change is necessary, explain why before making it.

## 11. Verification

Before considering a task complete:

1. Check that the implementation follows the existing architecture.
2. Check for compile-time issues.
3. Check nullability issues.
4. Check dependency injection registrations when new services are introduced.
5. Check authorization implications.
6. Check database/query behavior when EF Core is involved.
7. Check whether existing tests should be updated or new tests added.
8. Check for unintended changes to existing behavior.

Do not claim that code compiles, tests pass, or a command succeeded unless it was actually verified.

## 12. Communication

When explaining your work:

- Be concise and technical.
- Explain important decisions and trade-offs.
- Mention assumptions when they matter.
- Clearly identify anything that could not be verified.
- Do not invent APIs, classes, methods, configuration, or project behavior.
- If the repository does not provide enough information to safely implement something, inspect more of the codebase before making assumptions.

## 13. Priority of Information

When deciding how to implement something, use this priority:

1. Explicit requirements of the current task.
2. Existing project architecture and conventions.
3. Existing project code and established patterns.
4. Official C# / .NET / EF Core behavior.
5. General best practices.
6. Personal stylistic preferences.

Never override an explicit project requirement merely because another approach is generally preferred.

## 14. Final Rule

Do not optimize for producing the largest amount of code.

Optimize for producing the smallest correct, maintainable, and architecture-consistent change that fully solves the requested problem.
