## Summary

Describe the change in a few sentences:

- What problem does this PR solve?
- What is the main implementation approach?
- Why was this approach chosen?

## Type of change

Mark the relevant items:

- [ ] Bug fix
- [ ] New feature
- [ ] Refactor
- [ ] Documentation update
- [ ] Performance improvement
- [ ] Security improvement
- [ ] Dependency or tooling update
- [ ] Breaking change

## Architecture impact

Explain which parts of the solution are affected:

- [ ] Domain
- [ ] Application
- [ ] Infrastructure
- [ ] Gateway
- [ ] Module API
- [ ] Database or migrations
- [ ] Configuration
- [ ] Documentation

Notes:

<!-- Call out any layer-boundary decisions, module ownership concerns, or dependency changes. -->

## Modules affected

List the affected modules or features, for example:

- ProductModule
- IdentityModule
- CommunicationModule
- Shared Core Application or Infrastructure

## Validation and testing

List what you ran locally:

- [ ] `dotnet build src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj`
- [ ] `dotnet test tests/Alphabet.UnitTests/Alphabet.UnitTests.csproj`
- [ ] `dotnet test Alphabet.slnx`
- [ ] Manual API verification
- [ ] Swagger verification

Commands and results:

```text
Paste the commands you ran and the key outcome.
```

## Database and migrations

- [ ] No database impact
- [ ] Entity configuration changed
- [ ] Migration added
- [ ] Migration required but not added

Details:

<!-- Describe schema changes, seed changes, or provider-specific considerations. -->

## Configuration changes

- [ ] No configuration changes
- [ ] `appsettings.json` updated
- [ ] `appsettings.Development.json` updated
- [ ] New options/settings type added
- [ ] Secrets or environment variables required

Details:

<!-- Document any new settings and expected values. -->

## API and contract changes

- [ ] No API changes
- [ ] New endpoint added
- [ ] Existing endpoint behavior changed
- [ ] Contract or DTO changed
- [ ] Swagger documentation updated

Details:

<!-- Include example routes or request/response changes if useful. -->

## Documentation

- [ ] No documentation changes needed
- [ ] README updated
- [ ] README-AUTH updated
- [ ] XML comments added or updated
- [ ] Other docs updated

## Risks and follow-up

Call out anything reviewers should pay special attention to:

- edge cases
- backward compatibility concerns
- operational considerations
- future work intentionally left out of scope

## Reviewer notes

Anything specific you want reviewers to focus on:

<!-- Example: transaction boundaries, configuration naming, security assumptions, module ownership, migration strategy -->
