# Contributing to SBOM License Downloader

Thank you for your interest in contributing to SBOM License Downloader! This document provides guidelines and information for contributors.

## Project Structure

```
SBOM.Licenses/
├── src/
│   └── SBOM.Licenses/
│       ├── Configuration/
│       │   └── LicenseDownloaderConfig.cs
│       ├── Models/
│       │   ├── SbomComponent.cs
│       │   ├── CycloneDxModels.cs
│       │   └── SpdxModels.cs
│       ├── Services/
│       │   ├── SbomReader.cs
│       │   ├── LicenseDownloader.cs
│       │   ├── LicenseFileManager.cs
│       │   ├── GitHubLicenseService.cs
│       │   ├── PackageExclusionService.cs
│       │   └── LicenseDownloadOrchestrator.cs
│       ├── Program.cs
│       ├── appsettings.json
│       └── SBOM.Licenses.csproj
├── examples/
│   ├── example-sbom.json
│   ├── example-spdx-sbom.json
│   └── test-github-sbom.json
├── LICENSE
├── README.md
└── CONTRIBUTING.md
```

## Development Setup

### Prerequisites

- .NET 8.0 SDK or higher
- Git
- A code editor (Visual Studio, VS Code, or Rider recommended)

### Getting Started

1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/SBOM.Licenses
   cd SBOM.Licenses
   ```

3. Create a new branch for your feature or fix:
   ```bash
   git checkout -b feature/your-feature-name
   ```

## Development Workflow

### Building from Source

```bash
# Build the project
dotnet build

# Pack as NuGet package
dotnet pack -c Release

# Install locally for testing
dotnet tool install --global --add-source ./bin/Release SBOM.Licenses
```

### Running Tests

```bash
cd src/SBOM.Licenses
dotnet test
```

### Testing Your Changes

After making changes, test the tool locally:

```bash
# Uninstall previous version
dotnet tool uninstall --global SBOM.Licenses

# Build and install your version
cd src/SBOM.Licenses
dotnet pack -c Release
dotnet tool install --global --add-source ./bin/Release SBOM.Licenses

# Test with example SBOM
sbom-licenses ./examples/example-sbom.json
```

## Architecture Overview

### SBOM Format Support

The application supports multiple SBOM formats through a unified architecture:

1. **Format-Specific Models** (`Models/CycloneDxModels.cs`, `Models/SpdxModels.cs`)
   - Define C# classes that match the JSON structure of each SBOM format
   - Use `System.Text.Json.Serialization` attributes for JSON mapping

2. **Universal Component Model** (`Models/SbomComponent.cs`)
   - All SBOM formats are converted to this normalized internal representation
   - Contains: Name, Version, Licenses, PackageUrl, RepositoryUrl

3. **Format Detection & Parsing** (`Services/SbomReader.cs`)
   - Auto-detects SBOM format by checking for format-specific properties
   - Each format has its own parser method (e.g., `ParseCycloneDxAsync`, `ParseSpdxAsync`)
   - Converts format-specific models to `SbomComponent` list

4. **Format-Agnostic Processing**
   - Once converted to `SbomComponent`, all downstream services work identically
   - `LicenseDownloader`, `GitHubLicenseService`, `LicenseFileManager` are format-agnostic

### Adding a New SBOM Format

To add support for a new SBOM format:

1. Create format-specific models in `Models/YourFormatModels.cs`
2. Add format detection logic in `SbomReader.ReadSbomAsync()`
3. Implement `ParseYourFormatAsync()` method in `SbomReader`
4. Convert to `SbomComponent` objects
5. Add example SBOM file in `examples/`
6. Update documentation (README.md, CONTRIBUTING.md)

### SPDX Implementation Example

The SPDX implementation serves as a reference for adding new formats:

- **Models**: `SpdxDocument`, `SpdxPackage`, `SpdxExternalRef` (in `SpdxModels.cs`)
- **License Extraction**: Priority order: `licenseConcluded` → `licenseDeclared` → `licenseInfoFromFiles`
- **Special Values**: Filters out `NOASSERTION` and `NONE`
- **External References**:
  - Package URLs (purl): `referenceCategory: "PACKAGE-MANAGER"`, `referenceType: "purl"`
  - VCS URLs: `referenceCategory: "OTHER"`, `referenceType` in known VCS types (git, svn, hg, bzr, cvs)
- **URL Validation**: Uses URI parsing to prevent false positives in domain matching

## Code Style

The project uses standard .NET coding conventions:

- Use meaningful variable and method names
- Follow C# naming conventions (PascalCase for classes and methods, camelCase for local variables)
- Add XML documentation comments to public APIs
- Keep methods focused and single-purpose
- Use async/await for I/O operations

## Submitting Changes

1. Ensure your code builds without errors
2. Test your changes thoroughly
3. Commit your changes with a clear, descriptive commit message:
   ```bash
   git commit -m "feat: add support for SPDX format"
   ```

4. Push to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```

5. Create a pull request from your fork to the main repository

## Commit Message Guidelines

We follow conventional commit messages:

- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `refactor:` - Code refactoring
- `test:` - Adding or updating tests
- `chore:` - Maintenance tasks

Examples:
- `feat: add SPDX format support`
- `fix: handle missing license files gracefully`
- `docs: update installation instructions`

## Versioning and Releases

This project uses semantic versioning with automated version calculation from git tags. See [VERSIONING.md](VERSIONING.md) for detailed information about:

- How versioning works
- Creating releases
- CI/CD pipeline
- Publishing to NuGet

### Quick Release Guide

To create a new release:

```bash
# Tag the release
git tag v1.0.0
git push origin v1.0.0

# GitHub Actions will automatically:
# 1. Build the project
# 2. Create NuGet package
# 3. Publish to NuGet.org
# 4. Create GitHub release
```

## Roadmap

The following features and improvements are planned:

- [x] Full SPDX format support (SPDX 2.3) ✅
- [ ] Support for additional package sources (npm, Maven, PyPI)
- [ ] GUI version
- [ ] Docker container
- [ ] Parallel downloads with configurable concurrency
- [ ] Caching of previously downloaded licenses
- [ ] Export reports (CSV, JSON, HTML)

If you'd like to work on any of these features, please open an issue first to discuss the implementation approach.

## Getting Help

If you have questions or need help:

- Open an issue on GitHub
- Check existing issues and pull requests
- Review the [README.md](README.md) for usage information

## Code of Conduct

Please be respectful and constructive in all interactions. We are committed to providing a welcoming and inclusive environment for all contributors.

## License

By contributing to this project, you agree that your contributions will be licensed under the MIT License.
