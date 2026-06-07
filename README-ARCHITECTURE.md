# Alphabet Modular Architecture

## Overview

Alphabet is structured as a modular monolith. The `src/Core` projects provide shared kernel concerns, while `src/Modules` contains feature-owned code. Core code must stay generic and reusable; module code owns feature behavior, data models, policies, and endpoint surfaces.

## Core Projects

- `Alphabet.Domain` contains common domain primitives such as `BaseEntity`, aggregate root contracts, domain events, specifications, repository contracts, and domain exceptions.
- `Alphabet.Application` contains application contracts, MediatR behaviors, reusable result types, DTO envelopes, validation exceptions, and mapping helpers.
- `Alphabet.Infrastructure` contains common EF Core persistence adapters, shared services, dependency injection entrypoints, and migration helpers.
- `Alphabet.Shared` contains constants, general-purpose enums, extensions, and helper utilities that have no dependency on application, domain, infrastructure, or presentation code.

## Module Separation

Feature modules stay under `src/Modules`. A module can contain these internal layers:

- `Domain`: feature entities, value objects, enums, domain services, repository interfaces, and feature events.
- `Application`: commands, queries, handlers, validators, DTOs, and module-specific service contracts.
- `Infrastructure`: entity configurations, repositories, background jobs, options, and external adapters.
- `Api` or `Presentation`: endpoints, controllers, hubs, and request models.

Modules should not reference other modules directly. Use shared abstractions in `Alphabet.Application`, domain events, or small module facades registered through dependency injection.

## Dependency Graph

```text
Api Host
  -> Module Api/Presentation
  -> Module Application
  -> Module Domain
  -> Core: Alphabet.Application, Alphabet.Domain, Alphabet.Shared
  -> Infrastructure adapters

Alphabet.Infrastructure
  -> Alphabet.Application
  -> Alphabet.Domain
  -> Alphabet.Shared
```

Presentation is an edge layer. Domain must not depend on application, infrastructure, or presentation.

## Cross-Module Communication

Use one of these patterns:

- Domain events for asynchronous reactions to business facts.
- Application interfaces for shared capabilities such as notifications, current user, caching, email, file storage, and time.
- Module facades for stable synchronous queries between modules.

Avoid sharing feature entities between modules. Duplicate small read models when needed to preserve module autonomy.

## Adding a New Module

1. Create a module folder under `src/Modules/<ModuleName>Module`.
2. Add `Domain`, `Application`, `Infrastructure`, and `Api` folders.
3. Put reusable abstractions in Core only when at least two modules need them.
4. Register module services through a `ModuleRegistration` or module-specific extension class.
5. Add project or compile includes only in the owning layer.
6. Add unit tests for domain/application behavior and integration tests for persistence or host behavior.

Use `scripts/New-AlphabetModule.ps1` to scaffold the folder layout.

## Testing Strategy

- Domain tests should validate invariants and value-object behavior without infrastructure.
- Application tests should cover handlers with mocked module repositories and Core interfaces.
- Infrastructure tests should verify EF Core mappings, repositories, and external adapters.
- Host or functional tests should validate module registration and endpoint behavior.

## Migration Strategy

This repo currently composes modules into aggregate Core projects through compile includes. The migration path is incremental:

1. Keep existing namespaces and public contracts stable while extracting shared primitives.
2. Move generic abstractions into Core first.
3. Create physical module projects one module at a time.
4. Replace cross-module direct calls with Core interfaces, events, or facades.
5. Move EF configurations and repositories into module infrastructure projects after tests cover persistence behavior.
6. Update the host to call each module registration method.

## Current Refactor Notes

The first pass adds the missing shared-kernel primitives and documents the target boundaries while preserving the current buildable solution shape. Larger physical module-project extraction should be done module by module to avoid destabilizing the existing migrations and host wiring.
