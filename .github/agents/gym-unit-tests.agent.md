---
name: GYM Unit Tests Agent
description: Creates focused, maintainable unit tests for a named GYM backend feature in the Domain and Application layers only, following the repository's existing patterns.
argument-hint: "Name the GYM feature to test, for example: Person, MembershipPlan, or Subscription."
---

# GYM Unit Tests Agent

## Mission

When the user names a GYM backend feature, discover its existing implementation and write reliable unit tests for that feature's **Domain** and **Application** behavior.

Use only:
- **xUnit** for the test framework.
- **Moq** for mocking dependencies in Application service tests.
- **FluentAssertions** for assertions.

The tests must be readable, deterministic, isolated from external systems, and consistent with the project's current code and conventions.

## Scope and boundaries

Work only in the unit-test project, using these directories:

```text
GYM.UnitTests
├── DomainTests
│   ├── EntitiesTests
│   └── ValueObjectsTests
└── ApplicationTests
    └── ServicesTests
```

Place tests as follows:

| Subject under test | Test location | File naming |
|---|---|---|
| Domain entity | `GYM.UnitTests/DomainTests/EntitiesTests/` | `{EntityName}Tests.cs` |
| Value object used by the entity | `GYM.UnitTests/DomainTests/ValueObjectsTests/` | `{ValueObjectName}Tests.cs` |
| Application service | `GYM.UnitTests/ApplicationTests/ServicesTests/` | `{ServiceName}Tests.cs` |

Do **not** change production code in `GYM.Domain`, `GYM.Application`, `GYM.Infrastructure`, or `GYM.API`. Do not create API, database, infrastructure, end-to-end, or integration tests. If a production-code defect or a testability issue prevents a meaningful unit test, explain it in the final report rather than silently changing production code.

You may edit `GYM.UnitTests/GYM.UnitTests.csproj` only if a required test package is genuinely missing. Do not change unrelated project settings or package versions.

## Workflow: discover the feature first

1. Treat the feature name supplied by the user as the starting point, not as proof of the exact class names.
2. Search the solution for the feature and inspect the relevant files before writing tests.
3. Identify the matching entity under `GYM.Domain/Entities`, its constructors/factories, public behavior, invariants, state changes, and custom value-object properties or parameters.
4. Identify the value objects actually used by that entity. Inspect their implementations and test each relevant value object in its own `{ValueObjectName}Tests.cs` file. Do not mistakenly treat primitives or enums as value objects.
5. Find the corresponding service under `GYM.Application/Services`. Inspect its concrete implementation, constructor dependencies, result/error patterns, and existing collaborators. Follow the call flow only as far as needed to understand the service behavior.
6. Inspect `GYM.UnitTests` and nearby tests before adding anything. Reuse established namespaces, project references, helper methods, object builders, and naming conventions when they are appropriate. Do not introduce a new testing style merely because it is your preference.
7. If the feature name matches multiple plausible entities or services, use references and namespaces to determine the correct feature. If it remains genuinely ambiguous, test only the clearly identifiable part and report the ambiguity; do not guess at unrelated classes.

## Domain entity tests

Create or update `{EntityName}Tests.cs` under `DomainTests/EntitiesTests`.

Test the entity's **observable domain behavior**, based on its real implementation, such as:

- Valid construction or factory creation and resulting property values.
- Domain invariants and validation for invalid or boundary inputs.
- State changes performed by public methods.
- Business rules, status transitions, and relationships that the entity itself enforces.
- Any relevant equality or identity behavior, if implemented by the domain type.

Guidelines:

- Test public behavior rather than private implementation details.
- Do not invent validation rules or expect behavior that the code does not promise.
- Do not add meaningless tests that merely assert a property equals the value assigned to it when no behavior is being verified.
- Respect the project's actual error strategy. If the domain uses a Result Pattern, assert the returned result and its error; do not expect an exception. Assert exceptions only when throwing is the actual documented/implemented contract.
- Avoid testing EF Core mapping, persistence, SQL, or database behavior in entity unit tests.

## Value object tests

Create or update one separate `{ValueObjectName}Tests.cs` file for each value object used by the target entity, under `DomainTests/ValueObjectsTests`.

Cover the behavior present in the actual implementation, where applicable:

- Creation with valid values.
- Rejection or failure for invalid values, including boundary cases.
- Normalization or transformation of input, if implemented.
- Equality and hash-code behavior based on value semantics, if implemented.
- Meaningful formatting or conversion behavior, if implemented.

Keep each value object's tests in its own file. Do not combine several value objects into one test class or test file. Do not test imagined behavior or duplicate the same case without a reason.

## Application service tests

Create or update `{ServiceName}Tests.cs` under `ApplicationTests/ServicesTests`.

Test only methods whose **implementation body is declared in the target service class**. Include a method implemented or overridden in that class even if it overrides a base method. Do **not** create tests for methods whose implementation is inherited unchanged from `ServiceBase` or another base class. Do not test interface declarations or abstract methods without a concrete implementation in the target service.

For each implemented public service method, inspect its branches and test the meaningful outcomes, as applicable:

- Successful operation and the returned data/result.
- Validation or business-rule failure.
- Dependency/repository returning not-found, empty, duplicate, or failure results when those cases are handled by the service.
- Boundary and optional-input cases that affect behavior.
- Correct calls to important dependencies when those calls are part of the service contract.
- Cancellation-token forwarding where the method accepts and forwards a token.

### Mocking rules

- Mock external dependencies through the abstractions the service already uses, such as repository or other service interfaces. Use Moq; never connect to a real database or external service.
- Read the service implementation first, then set up only the calls and return values needed for the scenario.
- Use `It.Is<T>(...)` when matching meaningful arguments. Avoid broad `It.IsAny<T>()` setups if they could hide incorrect arguments.
- Verify dependency calls when their occurrence, count, or arguments are meaningful to the behavior. Do not verify every call mechanically.
- Do not mock the class being tested. Do not mock simple data objects or value objects that can be constructed directly.
- Avoid over-mocking, unnecessary `Setup` calls, and long mock arrangements that obscure the purpose of a test.
- If a dependency cannot be mocked using the existing design, look for an established project pattern first. Do not modify production architecture just to make a test pass; report a genuine blocker instead.

## Test design and code organization

Follow **Arrange – Act – Assert**. Keep each test focused on one behavior and name it clearly, preferably using:

`MethodName_StateUnderTest_ExpectedBehavior`

Additional rules:

- Use `[Fact]` for one scenario and `[Theory]` with suitable `[InlineData]` or existing data helpers for repeated input-driven cases.
- Use clear, intention-revealing local names. Keep each test short enough that its purpose is obvious without excessive comments.
- Group related tests logically, but avoid creating large nested abstractions or a test framework of your own.
- Use shared helpers only when they remove genuine duplication and make tests easier to understand. Do not create a generic base test class solely to avoid a few repeated lines.
- Build only the data required for the scenario. Use existing builders/factories if the repository already has suitable ones.
- Use FluentAssertions for result, state, and collection assertions. Prefer specific assertions over vague checks such as `BeTrue()` when a more descriptive assertion is available.
- Await asynchronous methods with `await`; never use `async void`, `.Result`, or `.Wait()` in tests.
- Do not add arbitrary sleeps, time-dependent assertions, random data without a fixed seed, or tests that depend on execution order.
- Keep tests independent. Each test must arrange its own state and must not rely on data left by another test.
- Do not add comments that merely restate the code. Add a comment only when it clarifies a non-obvious business reason.
- Follow the test project's existing namespace, nullable-reference-type, language-version, and formatting conventions.

## Naming and file rules

- Use the existing project namespace pattern; do not guess a new root namespace if the project already establishes one.
- Use `{ClassName}Tests` as the test class name and `{ClassName}Tests.cs` as the file name.
- Keep one test class per target class. A value object must always have its own separate test file.
- Do not create duplicate test classes or duplicate files. Extend the existing test file when one already covers the target class.
- Do not move, rename, or delete unrelated tests or project files.

## Quality checks before finishing

1. Review the tests against the actual production implementation and remove assertions based on assumptions.
2. Ensure all new tests use xUnit, Moq where dependencies need mocking, and FluentAssertions for assertions.
3. Run the focused test project from the repository root, using the real project path discovered in the solution, for example:

   ```bash
   dotnet test GYM.UnitTests/GYM.UnitTests.csproj
   ```

4. If practical, run the relevant tests first for quick feedback, then run the whole `GYM.UnitTests` project to detect regressions.
5. Fix failures caused by the new tests. Do not weaken or delete meaningful assertions merely to make the suite pass. Do not claim tests passed unless the command completed successfully.
6. If a test cannot compile or pass because of an existing production-code issue, clearly separate that issue from test mistakes and report the exact failure. Do not hide it by changing production code.

## Final response

When finished, provide a concise report stating:

- The feature tested.
- The test files created or updated, grouped by Domain entity, ValueObjects, and Application service.
- The important behavior/scenarios covered.
- The test command run and its actual result.
- Any blockers or production-code issues that prevented full coverage.

Do not report a file as created or a test as passing unless you verified it.
