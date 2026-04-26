# Contributing to Alphabet

Thank you for contributing to Alphabet. This repository is organized around Clean Architecture, modular boundaries, CQRS, and explicit cross-cutting concerns. Good contributions improve functionality without weakening those boundaries.

## Before you start

Please read:

- [README.md](./README.md)
- [README-AUTH.md](./README-AUTH.md)
- [CODE_OF_CONDUCT.md](./CODE_OF_CONDUCT.md)

Before opening a pull request, make sure the proposed change fits the existing architecture and does not introduce avoidable coupling between layers or modules.

## Project principles

Contributions should preserve these rules:

- `Domain` stays independent and does not reference Infrastructure or Presentation concerns.
- `Application` contains use cases, validation, DTOs, and contracts for cross-cutting services.
- `Infrastructure` implements Application abstractions and persistence/external integrations.
- `Gateway` is the composition root and API entry point.
- Feature modules under `src/Modules` own their own API surface and should not directly reference each other.
- Cross-module coordination should happen through events, abstractions, or message-based integration, not direct implementation coupling.

## Development workflow

1. Create a branch for your work.
2. Keep changes focused on a single concern.
3. Follow existing folder structure and naming patterns.
4. Add or update tests for behavior changes.
5. Update documentation when behavior, configuration, or developer workflow changes.
6. Open a pull request using the repository template.

## Local setup

Restore the solution:

```bash
dotnet restore Alphabet.slnx
```

Run the API:

```bash
dotnet run --project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
```

Optional local dependencies:

```bash
docker compose up -d
```

## Coding guidelines

### Architecture and boundaries

- Keep controller or endpoint logic thin.
- Route all business actions through MediatR commands or queries.
- Do not call `SaveChanges` inside repositories.
- Use `IUnitOfWork` for transaction boundaries.
- Prefer extending existing abstractions over bypassing them.
- Do not place domain rules in Infrastructure or endpoint code.

### Modules

When adding a new module:

1. Create `src/Modules/<ModuleName>`.
2. Add the module API endpoints in `Api`.
3. Add supporting Application slices under `src/Core/Alphabet.Application/Features/<FeatureName>`.
4. Register required infrastructure services in `src/Core/Alphabet.Infrastructure`.
5. Wire the module into `Program.cs`.
6. Update `Alphabet.slnx` and `README.md` when needed.

### Validation and errors

- Add FluentValidation validators for incoming commands and queries.
- Return `Result` or `Result<T>` for business outcomes.
- Reserve exceptions for unexpected failures and framework boundaries.
- Keep API responses consistent with the global exception middleware and Problem Details conventions.

### Documentation

This repository expects documentation to evolve with the code:

- Add XML comments to public types and methods that are part of the developer-facing surface.
- Add Swagger summaries and descriptions to minimal API endpoints.
- Update `README.md`, `README-AUTH.md`, or module-specific docs when configuration or usage changes.

## Testing expectations

At a minimum, contributors should run the most relevant automated checks for their change.

Common commands:

```bash
dotnet build src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
dotnet test tests/Alphabet.UnitTests/Alphabet.UnitTests.csproj
dotnet test Alphabet.slnx
```

Please include in the pull request description:

- what you ran
- what passed
- anything you could not run locally

## Database and migrations

If your change affects persistence:

- update EF Core entity configuration
- add or update migrations when required
- document any migration or configuration impact in the pull request

Example:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj \
  --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj \
  --output-dir Persistence/Migrations
```

## Reporting bugs

When filing a bug report, please include:

- expected behavior
- actual behavior
- steps to reproduce
- environment details
- relevant logs, screenshots, or stack traces

For security issues, do not create a public issue. Follow [SECURITY.md](./SECURITY.md).

## Feature proposals

When proposing a feature or enhancement:

- explain the problem being solved
- explain why the change belongs in Alphabet
- describe architectural impact
- describe configuration, migration, or backward compatibility considerations

The more concrete the proposal, the easier it is to review.

## Pull request checklist

Before submitting a pull request, confirm that you have:

- kept the change scoped and understandable
- added or updated tests where appropriate
- updated docs for behavioral or configuration changes
- preserved layer and module boundaries
- documented any known risks, limitations, or follow-up work

We appreciate thoughtful contributions that make the platform easier to extend, operate, and maintain.
