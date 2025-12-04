# SBOM License Downloader

A .NET tool that automatically reads SBOM (Software Bill of Materials) files and downloads the contained license files from the original packages.

## Features

- ✅ Automatically reads SBOM files (CycloneDX and SPDX format support)
- ✅ Downloads licenses directly from NuGet packages
- ✅ Supports direct license URLs
- ✅ Smart file naming: `PackageName-Version.extension`
- ✅ Configurable output directory
- ✅ Preserves original file extensions (or uses `.txt` as fallback)
- ✅ **Package exclusion patterns** - Skip framework packages (e.g., `Microsoft.*`, `System.*`)
- ✅ Comprehensive logging
- ✅ Configuration via `appsettings.json` or command-line arguments
- ✅ Install globally as a .NET tool (like `cyclonedx`)

## Requirements

- .NET 8.0 SDK or higher

## Installation

### Install as .NET Global Tool (Recommended)

Once published to NuGet.org:

```bash
dotnet tool install --global SBOM.Licenses
```

### Install from Source

```bash
# Clone repository
git clone https://github.com/dborgards/SBOM.Licenses
cd SBOM.Licenses

# Build and pack
cd src/SBOM.Licenses
dotnet pack -c Release

# Install locally
dotnet tool install --global --add-source ./bin/Release SBOM.Licenses
```

### Update the Tool

```bash
dotnet tool update --global SBOM.Licenses
```

### Uninstall the Tool

```bash
dotnet tool uninstall --global SBOM.Licenses
```

## Usage

### Basic Usage

```bash
# Download licenses from SBOM file
sbom-licenses ./sbom.json

# Specify custom output directory
sbom-licenses ./sbom.json ./licenses

# Use example SBOM
sbom-licenses ./examples/example-sbom.json ./my-licenses
```

### Configuration File

Create an `appsettings.json` in your working directory:

```json
{
  "LicenseDownloader": {
    "OutputDirectory": "./licenses",
    "SbomPath": "./sbom.json",
    "CreateOutputDirectoryIfNotExists": true,
    "OverwriteExistingFiles": false,
    "DefaultFileExtension": ".txt",
    "ExcludedPackagePatterns": [
      "Microsoft.*",
      "System.*"
    ]
  }
}
```

Then run without arguments:

```bash
sbom-licenses
```

## Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `OutputDirectory` | Target directory for downloaded licenses | `./licenses` |
| `SbomPath` | Path to SBOM file | `./sbom.json` |
| `CreateOutputDirectoryIfNotExists` | Automatically create output directory | `true` |
| `OverwriteExistingFiles` | Overwrite existing files | `false` |
| `DefaultFileExtension` | File extension for licenses without extension | `.txt` |
| `ExcludedPackagePatterns` | List of package name patterns to exclude (supports wildcards `*` and `?`) | `["Microsoft.*", "System.*"]` |

### Package Exclusion Patterns

You can exclude packages from license download using wildcard patterns. This is useful for framework packages that don't require separate license files:

```json
{
  "LicenseDownloader": {
    "ExcludedPackagePatterns": [
      "Microsoft.*",           // Excludes all Microsoft.* packages
      "System.*",              // Excludes all System.* packages
      "MyCompany.Internal.*",  // Excludes internal packages
      "Specific.Package"       // Excludes a specific package
    ]
  }
}
```

**Supported wildcards:**
- `*` - Matches any number of characters (e.g., `Microsoft.*` matches `Microsoft.Extensions.Logging`)
- `?` - Matches a single character (e.g., `Test?.Package` matches `Test1.Package`)

Pattern matching is **case-insensitive** and **culture-invariant**.

## SBOM Format Support

### CycloneDX (Fully Supported)

The application supports CycloneDX SBOM files in JSON format:

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

### SPDX (In Development)

SPDX format support is under development.

## File Naming Convention

Downloaded licenses are named according to this pattern:

```
{PackageName}-{Version}.{OriginalExtension}
```

Examples:
- `Newtonsoft.Json-13.0.3.txt`
- `System.Text.Json-8.0.0.md`
- `Microsoft.Extensions.Logging-8.0.0.txt`

If the original file has no extension, `.txt` is used (configurable).

## Download Strategies

The application attempts to find licenses in the following order:

1. **Direct License URL** - If a license URL is specified in the SBOM
2. **NuGet Package (via PURL)** - Extracts the license from the NuGet package (.nupkg)
3. **NuGet Package (via Name)** - Attempts to find the package by name and version

Within NuGet packages, the application searches for the following files:
- `LICENSE`
- `LICENSE.txt`
- `LICENSE.md`
- `License.txt`
- `LICENCE`
- `LICENCE.txt`
- `COPYING`
- `LICENSE-MIT`
- `LICENSE-APACHE`

## Example Output

```
SBOM License Downloader v1.0
=====================================
Configuration:
  SBOM Path: ./sbom.json
  Output Directory: ./licenses
  Default Extension: .txt
  Overwrite Existing: False
  Excluded Patterns: Microsoft.*, System.*

=== Starting License Download Process ===
SBOM Path: ./sbom.json
Step 1: Reading SBOM...
Detected CycloneDX format
Parsed 50 components from CycloneDX SBOM
Found 50 components in SBOM
Excluded 25 packages based on configured patterns
Processing 25 components (after exclusions)

Step 2: Downloading licenses...
Found license file in NuGet package: LICENSE.txt
Saved license file: ./licenses/Newtonsoft.Json-13.0.3.txt
...

=== Download Process Complete ===
Total components: 50
Excluded packages: 25
Successful downloads: 23
Failed downloads: 2
Total files: 23, Total size: 145.32 KB

=====================================
Summary:
  Total Components: 50
  Excluded Packages: 25
  Successful Downloads: 23
  Failed Downloads: 2
  Output Directory: ./licenses
=====================================
Process completed with errors
```

## Logging

The application uses Microsoft.Extensions.Logging with configurable log levels in `appsettings.json`:

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

Log Levels:
- `Trace` - Very detailed, all details
- `Debug` - Debug information
- `Information` - Standard, important information
- `Warning` - Warnings
- `Error` - Errors
- `Critical` - Critical errors

## Generating SBOM Files

To generate an SBOM file for your .NET project, you can use the following tools:

### Using dotnet CLI (.NET 7+):

```bash
# Generate SBOM during build
dotnet build /p:GenerateSBOM=true

# SBOM will be saved in obj/Debug/net8.0/sbom/
```

### Using CycloneDX Tool:

```bash
# Install tool
dotnet tool install --global CycloneDX

# Generate SBOM
dotnet CycloneDX ./YourProject.csproj -o ./sbom.json
```

## Troubleshooting

### "SBOM file not found"
- Check the path to your SBOM file
- Use absolute or relative paths correctly

### "Failed to download from NuGet"
- Check your internet connection
- Some packages don't include a license file in the package
- Older package versions might not be available anymore

### "No license file found in NuGet package"
- The package doesn't contain a standard license file
- Try specifying a license URL in the SBOM

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details on how to contribute to this project.

## Support

For questions or issues, please open an issue on GitHub.
