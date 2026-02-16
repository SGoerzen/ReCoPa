# ADR 0003: Runtime Plugin Loading From User Directories

Date: 2026-02-16  
Status: Accepted

## Context

Researchers and contributors need to extend the platform without rebuilding the core app. Plugins must be discoverable on all supported operating systems.

## Decision

Plugins are .NET assemblies discovered at startup from OS-specific plugin directories. The plugin SDK is provided in `ReCoPa.Plugins` and plugins implement `IPluginPackage`.

## Consequences

Plugin installation is a file copy into the plugin directory. The host must handle loading errors and plugin state, and plugin authors must target the SDK interfaces.

