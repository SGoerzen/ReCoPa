# ADR 0008: Use dotnet test For Unit Tests

Date: 2026-02-16  
Status: Accepted

## Context

The solution needs a consistent, standard test runner across platforms.

## Decision

Use `dotnet test` to run unit tests in `ReCoPa.Tests`.

## Consequences

All tests should be runnable from the command line using the .NET SDK, and CI can rely on a standard command.

