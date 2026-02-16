# ADR 0004: Use A Custom Socket Server For Live Data Ingestion

Date: 2026-02-16  
Status: Accepted

## Context

ReCoPa needs a real-time channel to receive live session data from XR clients and related tools with minimal latency.

## Decision

Use a custom socket server hosted in the app to receive and route events. The default port is `4567`.

## Consequences

Client integrations rely on the custom protocol and port configuration. The server lifecycle is tied to the app and must be managed on shutdown.

