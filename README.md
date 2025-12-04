# SBOM License Downloader

[![License](https://img.shields.io/badge/license-MIT-brightgreen.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/SBOM.Licenses.svg)](https://www.nuget.org/packages/SBOM.Licenses/)
[![CI](https://github.com/dborgards/SBOM.Licenses/actions/workflows/ci.yml/badge.svg)](https://github.com/dborgards/SBOM.Licenses/actions/workflows/ci.yml)
[![Release](https://github.com/dborgards/SBOM.Licenses/actions/workflows/release.yml/badge.svg)](https://github.com/dborgards/SBOM.Licenses/actions/workflows/release.yml)

A .NET tool that automatically reads SBOM (Software Bill of Materials) files and downloads the contained license files from the original packages.

## Features

- ✅ Automatically reads SBOM files (CycloneDX and SPDX format support)
- ✅ **GitHub API integration** - Downloads licenses directly from GitHub repositories (faster than NuGet)
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
    ],
    "GitHubToken": "ghp_your_token_here"
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
| `GitHubToken` | GitHub Personal Access Token for API requests (optional, can also be set via `GITHUB_TOKEN` environment variable) | `null` |

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

### GitHub API Authentication

To increase the GitHub API rate limit from 60 to 5,000 requests per hour, you can provide a Personal Access Token (PAT) in two ways:

#### Option 1: Environment Variable (Recommended)

```bash
# Linux/macOS
export GITHUB_TOKEN=ghp_your_token_here
sbom-licenses ./sbom.json

# Windows PowerShell
$env:GITHUB_TOKEN="ghp_your_token_here"
sbom-licenses ./sbom.json

# Windows CMD
set GITHUB_TOKEN=ghp_your_token_here
sbom-licenses ./sbom.json
```

#### Option 2: Configuration File

Add to your `appsettings.json`:

```json
{
  "LicenseDownloader": {
    "GitHubToken": "ghp_your_token_here"
  }
}
```

#### Creating a GitHub Personal Access Token

1. Go to GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Click "Generate new token"
3. Select scopes: **No scopes required** (public repository access only)
4. Generate and copy the token
5. Set it via environment variable or config file

**Note:** The token does not require any special scopes since it's only used for reading public repository licenses via the GitHub API.

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
      ],
      "externalReferences": [
        {
          "type": "vcs",
          "url": "https://github.com/JamesNK/Newtonsoft.Json"
        }
      ]
    }
  ]
}
```

The `externalReferences` field with `type: "vcs"` enables GitHub API integration for faster license downloads.

### SPDX (Fully Supported)

The application fully supports SPDX 2.3 SBOM files in JSON format:

```json
{
  "spdxVersion": "SPDX-2.3",
  "dataLicense": "CC0-1.0",
  "SPDXID": "SPDXRef-DOCUMENT",
  "name": "Example SPDX SBOM",
  "documentNamespace": "https://example.org/sbom/example-1.0.0",
  "packages": [
    {
      "SPDXID": "SPDXRef-Package-Newtonsoft.Json",
      "name": "Newtonsoft.Json",
      "versionInfo": "13.0.3",
      "downloadLocation": "https://github.com/JamesNK/Newtonsoft.Json",
      "licenseConcluded": "MIT",
      "licenseDeclared": "MIT",
      "externalRefs": [
        {
          "referenceCategory": "PACKAGE-MANAGER",
          "referenceType": "purl",
          "referenceLocator": "pkg:nuget/Newtonsoft.Json@13.0.3"
        },
        {
          "referenceCategory": "OTHER",
          "referenceType": "git",
          "referenceLocator": "https://github.com/JamesNK/Newtonsoft.Json"
        }
      ]
    }
  ]
}
```

**SPDX-specific features:**
- Extracts licenses from `licenseConcluded`, `licenseDeclared`, or `licenseInfoFromFiles`
- Automatically filters SPDX special values (`NOASSERTION`, `NONE`)
- Supports package URLs (purl) via `externalRefs` with category `PACKAGE-MANAGER`
- Extracts repository URLs from `externalRefs` with category `OTHER` and VCS types (git, svn, hg, bzr, cvs)
- Falls back to `downloadLocation` for repository detection
- Supports GitHub API integration via repository URLs

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

1. **GitHub API** - If a repository URL is specified in the SBOM's external references (type: `vcs`) and points to GitHub
2. **Direct License URL** - If a license URL is specified in the SBOM
3. **NuGet Package (via PURL)** - Extracts the license from the NuGet package (.nupkg)
4. **NuGet Package (via Name)** - Attempts to find the package by name and version

### GitHub API Integration

When a package has a GitHub repository URL in the SBOM (via `externalReferences` with type `vcs`), the application will:
- Call the GitHub API: `GET https://api.github.com/repos/{owner}/{repo}/license`
- Decode the Base64-encoded license content
- Use the actual license file from the repository (most authoritative source)
- Extract SPDX license ID from the GitHub response

**Benefits:**
- ⚡ **Faster** than downloading and extracting .nupkg files (several MB)
- 🎯 **More accurate** - Gets license directly from the source repository
- ✅ **Up-to-date** - Repository license may be more current than last NuGet release

**Rate Limits:**
- Without token: 60 requests/hour
- With GitHub Personal Access Token: 5,000 requests/hour

**Supported URL formats:**
- `https://github.com/owner/repo`
- `https://github.com/owner/repo.git`
- `git@github.com:owner/repo.git`
- `github.com/owner/repo`

### Within NuGet packages

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
  GitHub API: Enabled (with token)
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
Successfully retrieved license from GitHub: JamesNK/Newtonsoft.Json (SPDX: MIT)
Saved license file: ./licenses/Newtonsoft.Json-13.0.3.txt
Found license file in NuGet package: LICENSE.txt
Saved license file: ./licenses/SomePackage-1.0.0.txt
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
