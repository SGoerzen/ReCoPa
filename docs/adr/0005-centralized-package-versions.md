# ADR 0005: Centralize NuGet Package Versions

Date: 2026-02-16  
Status: Accepted

## Context

The solution contains multiple projects that must share compatible dependency versions.

## Decision

Manage NuGet package versions centrally using `Directory.Packages.props`.

## Consequences

Dependency updates are made in one place, and project files should avoid local version overrides unless required.

