# ADR 0007: Separate Desktop Bootstrapper Project

Date: 2026-02-16  
Status: Accepted

## Context

The solution needs a dedicated entry point for OS-specific desktop initialization while keeping the core app reusable.

## Decision

Maintain a separate `ReCoPa.Desktop` project as the desktop bootstrapper that references the core `ReCoPa` project.

## Consequences

Desktop-specific configuration lives in `ReCoPa.Desktop`, while core logic stays in `ReCoPa`.

