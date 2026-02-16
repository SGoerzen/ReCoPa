# ADR 0001: Use Avalonia For Cross-Platform UI

Date: 2026-02-16  
Status: Accepted

## Context

ReCoPa targets Windows, macOS, and Linux with a single desktop UI codebase. The UI needs a XAML-style layout, data binding support, and good .NET integration.

## Decision

Use Avalonia for the cross-platform UI framework and XAML for views.

## Consequences

ReCoPa depends on the Avalonia ecosystem and its XAML toolchain. UI work follows Avalonia patterns and tooling, and compiled bindings are preferred for performance.

