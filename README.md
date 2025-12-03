# SBOM License Downloader

Eine C# / .NET 10 Anwendung, die automatisch SBOM-Dateien (Software Bill of Materials) liest und die enthaltenen Lizenzen als Dateien aus den originalen Packages herunterlädt.

## Features

- ✅ Automatisches Lesen von SBOM-Dateien (CycloneDX und SPDX Format)
- ✅ Download von Lizenzen direkt aus NuGet-Packages
- ✅ Unterstützung für direkte Lizenz-URLs
- ✅ Intelligente Dateinamen: `PackageName-Version.extension`
- ✅ Konfigurierbarer Output-Ordner
- ✅ Beibehaltung der originalen Dateiendung (oder `.txt` als Fallback)
- ✅ Umfangreiches Logging
- ✅ Konfiguration über `appsettings.json` oder Command-Line

## Anforderungen

- .NET 10.0 SDK oder höher

## Installation

```bash
# Repository klonen
git clone <repository-url>
cd SBOM.Licenses

# Projekt bauen
cd src/SBOM.Licenses
dotnet build

# Optional: Veröffentlichen für Deployment
dotnet publish -c Release -o ./publish
```

## Verwendung

### Methode 1: Command-Line Argumente

```bash
# Einfachste Verwendung
dotnet run --project src/SBOM.Licenses -- ./sbom.json

# Mit benutzerdefiniertem Output-Ordner
dotnet run --project src/SBOM.Licenses -- ./sbom.json ./my-licenses

# Nach dem Publish
./publish/SBOM.Licenses ./sbom.json ./licenses
```

### Methode 2: Konfiguration über appsettings.json

Bearbeiten Sie die `appsettings.json`:

```json
{
  "LicenseDownloader": {
    "OutputDirectory": "./licenses",
    "SbomPath": "./sbom.json",
    "CreateOutputDirectoryIfNotExists": true,
    "OverwriteExistingFiles": false,
    "DefaultFileExtension": ".txt"
  }
}
```

Dann einfach ausführen:

```bash
dotnet run --project src/SBOM.Licenses
```

## Konfigurationsoptionen

| Option | Beschreibung | Standard |
|--------|--------------|----------|
| `OutputDirectory` | Zielordner für die heruntergeladenen Lizenzen | `./licenses` |
| `SbomPath` | Pfad zur SBOM-Datei | `./sbom.json` |
| `CreateOutputDirectoryIfNotExists` | Ordner automatisch erstellen | `true` |
| `OverwriteExistingFiles` | Vorhandene Dateien überschreiben | `false` |
| `DefaultFileExtension` | Dateiendung für Lizenzen ohne Extension | `.txt` |

## SBOM Format Support

### CycloneDX (vollständig unterstützt)

Die Anwendung unterstützt CycloneDX SBOM-Dateien im JSON-Format:

```json
{
  "bomFormat": "CycloneDX",
  "specVersion": "1.4",
  "components": [
    {
      "name": "Newtonsoft.Json",
      "version": "13.0.3",
      "purl": "pkg:nuget/Newtonsoft.Json@13.0.3",
      "licenses": [
        {
          "license": {
            "id": "MIT"
          }
        }
      ]
    }
  ]
}
```

### SPDX (in Entwicklung)

SPDX-Unterstützung ist in Arbeit.

## Dateinamens-Konvention

Die heruntergeladenen Lizenzen werden nach folgendem Muster benannt:

```
{PackageName}-{Version}.{OriginalExtension}
```

Beispiele:
- `Newtonsoft.Json-13.0.3.txt`
- `System.Text.Json-10.0.0.md`
- `Microsoft.Extensions.Logging-10.0.0.txt`

Falls die Originaldatei keine Endung hat, wird `.txt` verwendet (konfigurierbar).

## Download-Strategien

Die Anwendung versucht Lizenzen in folgender Reihenfolge zu finden:

1. **Direkte Lizenz-URL** - Falls in der SBOM eine Lizenz-URL angegeben ist
2. **NuGet Package (via PURL)** - Extrahiert die Lizenz aus dem NuGet-Package (.nupkg)
3. **NuGet Package (via Name)** - Versucht das Package über Name und Version zu finden

Innerhalb der NuGet-Packages sucht die Anwendung nach folgenden Dateien:
- `LICENSE`
- `LICENSE.txt`
- `LICENSE.md`
- `License.txt`
- `LICENCE`
- `LICENCE.txt`
- `COPYING`
- `LICENSE-MIT`
- `LICENSE-APACHE`

## Beispiel-Output

```
SBOM License Downloader v1.0
=====================================
Configuration:
  SBOM Path: ./sbom.json
  Output Directory: ./licenses
  Default Extension: .txt
  Overwrite Existing: False

=== Starting License Download Process ===
SBOM Path: ./sbom.json
Step 1: Reading SBOM...
Detected CycloneDX format
Parsed 25 components from CycloneDX SBOM
Found 25 components in SBOM

Step 2: Downloading licenses...
Found license file in NuGet package: LICENSE.txt
Saved license file: ./licenses/Newtonsoft.Json-13.0.3.txt
...

=== Download Process Complete ===
Successful: 23/25
Failed: 2/25
Total files: 23, Total size: 145.32 KB

=====================================
Summary:
  Total Components: 25
  Successful Downloads: 23
  Failed Downloads: 2
  Output Directory: ./licenses
=====================================
Process completed with errors
```

## Logging

Die Anwendung verwendet Microsoft.Extensions.Logging mit konfigurierbaren Log-Levels in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "System": "Warning",
      "Microsoft": "Warning"
    }
  }
}
```

Log-Levels:
- `Trace` - Sehr detailliert, alle Details
- `Debug` - Debug-Informationen
- `Information` - Standard, wichtige Informationen
- `Warning` - Warnungen
- `Error` - Fehler
- `Critical` - Kritische Fehler

## Projektstruktur

```
SBOM.Licenses/
├── src/
│   └── SBOM.Licenses/
│       ├── Configuration/
│       │   └── LicenseDownloaderConfig.cs
│       ├── Models/
│       │   ├── SbomComponent.cs
│       │   └── CycloneDxModels.cs
│       ├── Services/
│       │   ├── SbomReader.cs
│       │   ├── LicenseDownloader.cs
│       │   ├── LicenseFileManager.cs
│       │   └── LicenseDownloadOrchestrator.cs
│       ├── Program.cs
│       ├── appsettings.json
│       └── SBOM.Licenses.csproj
├── examples/
│   └── example-sbom.json
├── LICENSE
└── README.md
```

## Beispiel SBOM generieren

Um eine SBOM-Datei für Ihr .NET-Projekt zu generieren, können Sie folgende Tools verwenden:

### Mit dotnet CLI (ab .NET 7):

```bash
# SBOM während des Builds generieren
dotnet build /p:GenerateSBOM=true

# Die SBOM wird in obj/Debug/net10.0/sbom/ gespeichert
```

### Mit CycloneDX Tool:

```bash
# Tool installieren
dotnet tool install --global CycloneDX

# SBOM generieren
dotnet CycloneDX ./YourProject.csproj -o ./sbom.json
```

## Fehlerbehebung

### "SBOM file not found"
- Prüfen Sie den Pfad zur SBOM-Datei
- Verwenden Sie absolute oder relative Pfade korrekt

### "Failed to download from NuGet"
- Prüfen Sie die Internetverbindung
- Manche Packages haben keine Lizenz-Datei im Package
- Alte Package-Versionen könnten nicht mehr verfügbar sein

### "No license file found in NuGet package"
- Das Package enthält keine Standard-Lizenzdatei
- Versuchen Sie, eine Lizenz-URL im SBOM anzugeben

## Entwicklung

### Tests ausführen

```bash
cd src/SBOM.Licenses
dotnet test
```

### Code-Stil

Das Projekt verwendet Standard .NET Coding Conventions.

## Lizenz

Dieses Projekt ist unter der MIT-Lizenz lizenziert. Siehe die [LICENSE](LICENSE) Datei für Details.

## Beiträge

Beiträge sind willkommen! Bitte erstellen Sie einen Pull Request oder öffnen Sie ein Issue.

## Roadmap

- [ ] SPDX Format vollständig unterstützen
- [ ] Support für weitere Package-Quellen (npm, Maven, PyPI)
- [ ] GUI-Version
- [ ] Docker-Container
- [ ] Parallele Downloads mit konfigurierbarer Concurrency
- [ ] Caching von bereits heruntergeladenen Lizenzen
- [ ] Export-Report (CSV, JSON, HTML)

## Support

Bei Fragen oder Problemen öffnen Sie bitte ein Issue auf GitHub.
