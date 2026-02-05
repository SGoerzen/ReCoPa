# 🔬 ReCoPa v2 - Researcher Companion Panel

[![DOI](https://zenodo.org/badge/1143544056.svg)](https://doi.org/10.5281/zenodo.18496218)

**Eine moderne Desktop-Plattform für Echtzeit-Datenerfassung, Visualisierung und Analyse von Extended Reality (XR) Learning Sessions.**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-UI-0078D4?style=flat-square)](https://avaloniaui.net/)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey?style=flat-square)](#)

---

## 📋 Übersicht

ReCoPa ist eine **hochgradig erweiterbare Forschungs-Plattform** für die Echtzeit-Datenerfassung von **XR-Geräten und Learning Analytics Systemen**. Die Anwendung ermöglicht es Forschern, komplexe Lernprozesse zu visualisieren, zu analysieren und durch ein modulares Plugin-System zu erweitern.

**Ideal für:**
- 🎓 Learning Analytics & Educational Research
- 🥽 Extended Reality (VR/AR/MR) Experimente
- 📊 Echtzeit-Datenvisualisierung
- 🔌 Ecosystem-Integration via xAPI/LRS
- 🧩 Custom Analytics durch Plugins

---

## ✨ Features

- **🖥️ Cross-Platform Desktop UI** - Läuft nativ auf Windows, macOS & Linux (Avalonia)
- **🔌 Plugin-Architektur** - Erweitere die Plattform ohne Source Code zu ändern
- **🌐 Socket-Server** - Echtzeit-Kommunikation mit Clients (Custom Binary Protocol)
- **📈 Live Visualisierungen** - LiveCharts für Performance-optimierte Grafiken
- **🎯 xAPI Integration** - Learning Record Store (LRS) Support out-of-the-box
- **🎨 Modernes UI** - SukiUI Theme mit Toast-Benachrichtigungen
- **⚡ Reactive Architecture** - reaktive Datenströme mit ReactiveUI/RxJS
- **🔄 Hot Reload** - Änderungen während der Entwicklung live laden
- **🛡️ Type-Safe** - Nullable Reference Types, strenge Typprüfung

---

## 🚀 Quickstart

### Voraussetzungen

- **.NET 10.0** oder höher ([Download](https://dotnet.microsoft.com/download))
- **macOS**, **Windows** oder **Linux**

### Installation & Ausführung

1. **Repository klonen:**
   ```bash
   git clone <repository-url>
   cd recopa
   ```

2. **Dependencies installieren:**
   ```bash
   dotnet restore
   ```

3. **Anwendung starten (Normal):**
   ```bash
   dotnet run --project ReCoPa.Desktop
   ```

4. **Mit Hot Reload während der Entwicklung starten:**
   ```bash
   dotnet watch --project ReCoPa.Desktop run
   ```

   Der Socket-Server startet automatisch auf **Port 4567**.

---

## 🏗️ Projektstruktur

```
recopa/
├── ReCoPa/                    # Hauptanwendung (UI + Logik)
│   ├── App.axaml             # Application Entry Point
│   ├── ViewModels/           # MVVM Layer - Business Logik
│   ├── Views/                # Avalonia XAML UI
│   ├── Models/               # Datenmodelle
│   ├── Network/              # Socket Server & Kommunikation
│   ├── Plugins/              # Plugin-Verwaltung
│   ├── Services/             # Geschäftslogik-Services
│   ├── Converters/           # XAML Value Converters
│   └── Styles/               # Theme & Styling
│
├── ReCoPa.Desktop/           # Desktop-spezifischer Entry Point
│   └── Program.cs            # Windows/macOS/Linux Bootstrap
│
├── ReCoPa.Plugins/           # Plugin SDK/Interfaces
│   ├── IPluginPackage.cs     # Plugin Metadaten
│   ├── IVisualization.cs     # Visualisierungs-API
│   ├── IEndpoint.cs          # Daten-Endpoints
│   ├── IDataSource.cs        # Daten-Provider
│   └── IFilter.cs            # Datenfilter-API
│
├── ReCoPa.xAPI/              # xAPI/LRS Plugin (Beispiel)
│   ├── PluginPackage.cs      # Plugin-Definition
│   ├── LearningRecordStore.cs
│   └── Plugins/              # xAPI Komponenten
│       ├── ActivityPulse.cs
│       ├── FocusDistribution.cs
│       ├── TaskState.cs
│       └── xApiPreview.cs
│
└── ReCoPa.Tests/             # Unit Tests
```

---

## 🔌 Plugin-System

### Plugins erstellen

Plugins sind **.NET Assemblies** mit Metadaten-Implementierungen:

```csharp
public class MyPluginPackage : IPluginPackage
{
    public string Id => "com.example.myplugin";
    public string Name => "My Awesome Plugin";
    public string Description => "Ein Custom Analytics Plugin";
    
    public IPluginComponent[] Components => new[]
    {
        new MyVisualization(),
        new MyDataEndpoint(),
    };
    
    public Contributor[] Contributors => new[]
    {
        new Contributor { Name = "Max Mustermann", Github = "..." }
    };
    
    // ... weitere Eigenschaften
}
```

### Plugin-Installation

Plugins werden automatisch aus dem Plugin-Verzeichnis geladen:

**macOS/Linux:**
```
~/Library/Application Support/ReCoPa/Plugins/  (macOS)
~/.local/share/ReCoPa/Plugins/                  (Linux)
```

**Windows:**
```
%APPDATA%\ReCoPa\Plugins\
```

Einfach die DLL ins Verzeichnis kopieren - ReCoPa lädt sie automatisch beim Start.

---

## 🛠️ Entwicklung

### Architektur-Übersicht

```
┌─────────────────────────────────┐
│    Avalonia UI / XAML Views     │
├─────────────────────────────────┤
│     ReactiveUI ViewModels       │ ← MVVM Pattern
├─────────────────────────────────┤
│    Network / SocketServer       │ ← Clients verbinden
├─────────────────────────────────┤
│  Plugin Manager / Plugin Loader │ ← Dynamische Erweiterung
└─────────────────────────────────┘
```

### Tech Stack

| Layer | Technologie | Purpose |
|-------|-------------|---------|
| **UI** | Avalonia + XAML | Cross-Platform Desktop Framework |
| **Architektur** | ReactiveUI | Reactive MVVM, Dependency Handling |
| **Async** | Reactive Extensions | Streams, Observables |
| **Theming** | SukiUI | Modern Fluent-inspired Design |
| **Charts** | LiveCharts Core + SkiaSharp | Performance-optimierte Grafiken |
| **Server** | Custom Socket Server | Datenverbindung mit Clients |
| **Plugins** | .NET Assembly Loading | Runtime Extensibility |

### Code-Standards

- ✅ **Nullable Reference Types** - Durchgehend aktiviert
- ✅ **Latest C# Features** - `record`, `init`, `required`, etc.
- ✅ **Compiled Avalonia Bindings** - Bessere Performance & Compile-Time Checks
- ✅ **Reactive Programming** - `INotifyPropertyChanged` via ReactiveUI
- ✅ **MVVM Pattern** - `ViewModelBase` für alle VMs

### Performance-Tipps

- **Avalonia Compiled Bindings** nutzen (statt dynamische Bindings)
- **ObservableCollections** für UI-Listen (auto-sync)
- **Reactive Streams** für Echtzeit-Updates konfigurieren
- **Plugins** sollten lange Operationen async machen

---

## 🔧 Konfiguration

### Server-Optionen

Der Socket-Server wird in `App.OnFrameworkInitializationCompleted()` konfiguriert:

```csharp
Socket = new SocketServerHost(
    options: new SocketServerOptions { /* ... */ },
    uiPost: a => Dispatcher.UIThread.Post(a)  // UI-Thread-safety
);

await Socket.StartAsync(4567);  // Port 4567
```

### Plugin-States

Plugins können ihren State in `PluginStateStore` speichern:

```csharp
var stateStore = new PluginStateStore(pluginDirectory);
// Plugin-Konfiguration persistent speichern
```

---

## 📦 Dependencies

Das Projekt nutzt zentrale Dependency Management via `Directory.Packages.props`:

```xml
<ItemGroup>
    <PackageReference Include="Avalonia" Version="..." />
    <PackageReference Include="ReactiveUI" Version="..." />
    <PackageReference Include="SukiUI" Version="..." />
    <PackageReference Include="LiveChartsCore.SkiaSharpView.Avalonia" />
    <!-- ... weitere Packages -->
</ItemGroup>
```

Alle NuGet-Versionen zentral verwalten → besser für Stabilität.

---

## 🧪 Testing

Tests sind in `ReCoPa.Tests/` organisiert. Führe sie aus mit:

```bash
dotnet test
```

---

## 🎯 Nächste Schritte

- [ ] Dark Mode / Light Mode Theme Toggle
- [ ] Plugin Marketplace & Auto-Update
- [ ] Session Recording & Playback
- [ ] Advanced Filtering UI
- [ ] Performance Profiling Tools
- [ ] Cloud Sync für Daten-Backups

---

## 📝 Lizenz

Dieses Projekt ist unter der **MIT License** lizenziert. Siehe [LICENSE](LICENSE) für Details.

---

## 👤 Kontakt & Beiträge

**Lead Developer:** Sergej Görzen  
📧 [goerzen@cs.rwth-aachen.de](mailto:goerzen@cs.rwth-aachen.de)  
🐙 [GitHub](https://github.com/SGoerzen)

**Projekt Links:**
- 🌐 Website: [omilaxr.dev/recopa](https://omilaxr.dev/recopa)
- 📦 Repository: [github.com/SGoerzen/ReCoPa](https://github.com/SGoerzen/ReCoPa)
- 📚 xAPI Plugin: [github.com/SGoerzen/ReCoPa.xAPI](https://github.com/SGoerzen/ReCoPa.xAPI)

---

## 🙏 Acknowledgments

Gebaut mit ❤️ für Forschung & Extended Reality Learning.

Built with [Avalonia](https://avaloniaui.net/) | Powered by [.NET](https://dotnet.microsoft.com/) | Featured on [SukiUI](https://draycen.top)
