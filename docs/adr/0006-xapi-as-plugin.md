# ADR 0006: Ship xAPI Integration As A Plugin Bundle

Date: 2026-02-16  
Status: Accepted

## Context

Not all deployments require xAPI or LRS connectivity, and the integration should be optional.

## Decision

Implement xAPI/LRS support as a plugin package (`ReCoPa.xAPI`) rather than a core dependency.

## Consequences

xAPI functionality can be enabled or replaced without modifying the core app. The plugin package must be kept compatible with the plugin SDK.

