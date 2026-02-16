ReCoPa Plugins SDK
==================

This folder contains the public plugin SDK used by ReCoPa. The goal is to make plugin creation simple and predictable:

- Plugins are regular .NET assemblies (.dll) that implement `IPluginPackage`.
- Components inside a package can be visualizations, filters, or endpoints.
- Plugins can receive live data and query a rolling data store.


Quick Start
-----------

1) Create a new class library targeting the same framework as the host (currently `net10.0`).
2) Reference `ReCoPa.Plugins`.
3) Implement `IPluginPackage` (or derive from `PluginPackageBase`).
4) Implement components such as `IVisualization`.
5) Copy your built `.dll` into the ReCoPa plugins directory.

Plugin directory:

- Windows: `%AppData%\\ReCoPa\\Plugins`
- macOS: `~/Library/Application Support/ReCoPa/Plugins`
- Linux: `~/.local/share/ReCoPa/Plugins`


Minimal Plugin Package
----------------------

```csharp
using ReCoPa.Plugins;

public sealed class MyPluginPackage : PluginPackageBase
{
    public override string Id => "com.example.recopa.myplugin";
    public override string Name => "My Plugin";
    public override Contributor[] Contributors =>
    [
        new Contributor { Name = "Your Name", Email = "you@example.org" }
    ];
    public override string Description => "Example plugin.";
    public override IPluginComponent[] Components => [ new MyVisualization() ];
    public override string Website => "https://example.org";
    public override string Repository => "https://example.org/repo";
    public override string ChangelogUrl => "https://example.org/changelog";
}
```


Creating a Visualization
------------------------

Visualizations must return an Avalonia `Control` from `CreateView()`:

```csharp
using ReCoPa.Plugins;
using Avalonia.Controls;

public sealed class MyVisualization : IVisualization
{
    public string Name => "My Viz";

    public object CreateView()
    {
        return new TextBlock { Text = "Hello from plugin." };
    }
}
```


Receiving Live Data (Push)
--------------------------

If your component wants live data, implement `IDataConsumer`:

```csharp
using ReCoPa.Plugins;

public sealed class MyVisualization : IVisualization, IDataConsumer
{
    public string Name => "My Viz";
    public object CreateView() => new TextBlock();

    public void OnData(DataPacket data)
    {
        // data.EventName, data.Payload, data.TimestampUtc
    }
}
```

Notes:

- `DataPacket.Payload` is the raw JSON string from the socket.
- `DataPacket.EventName` is the socket event (e.g. `statements`).
- `OnData` is invoked on the main UI thread when possible; still keep work light.


Querying the Data Store
-----------------------

If your component wants to query recent data, implement `IDataAccessConsumer`:

```csharp
using ReCoPa.Plugins;

public sealed class MyVisualization : IVisualization, IDataAccessConsumer
{
    private IDataAccess? _access;
    public string Name => "My Viz";
    public object CreateView() => new TextBlock();

    public void SetDataAccess(IDataAccess access)
    {
        _access = access;

        var recent = access.Store.Query(new DataQuery
        {
            EventName = "statements",
            Limit = 200,
            NewestFirst = true
        });
    }
}
```

The store is a rolling in-memory buffer. Default retention currently keeps the last 10,000 packets.


Combining Push + Query
----------------------

You can implement both `IDataConsumer` and `IDataAccessConsumer`:

```csharp
public sealed class MyVisualization : IVisualization, IDataConsumer, IDataAccessConsumer
{
    public string Name => "My Viz";
    public object CreateView() => new TextBlock();

    public void SetDataAccess(IDataAccess access)
    {
        // initial load or backfill
    }

    public void OnData(DataPacket data)
    {
        // live updates
    }
}
```


Storing Plugin Settings
-----------------------

Plugins can persist settings via `PluginPackageBase`:

```csharp
public sealed class MyPluginPackage : PluginPackageBase
{
    public override string Id => "com.example.recopa.myplugin";
    public override string Name => "My Plugin";
    public override Contributor[] Contributors => [ new Contributor { Name = "You" } ];
    public override string Description => "Example plugin.";
    public override IPluginComponent[] Components => [ new MyVisualization() ];
    public override string Website => "https://example.org";
    public override string Repository => "https://example.org/repo";
    public override string ChangelogUrl => "https://example.org/changelog";

    public void SaveMySettings(MySettings settings)
        => SaveSettings("settings.json", settings);

    public MySettings LoadMySettings()
        => LoadSettings("settings.json", new MySettings());
}
```


Performance Tips
----------------

- Avoid expensive work in `OnData`; parse once and update minimal state.
- For UI updates, batch changes where possible.
- Use `DataQuery.Limit` to avoid processing huge datasets.
- Keep your view models small and incrementally update collections instead of rebuilding.


Troubleshooting
---------------

- If your view does not render, ensure `CreateView()` returns an Avalonia `Control`.
- If your plugin does not load, check that the assembly targets the same framework and is copied to the plugins directory.
- If you depend on extra assemblies, ship them alongside your plugin DLL.
