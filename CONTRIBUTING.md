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
│       │   └── CycloneDxModels.cs
│       ├── Services/
│       │   ├── SbomReader.cs
│       │   ├── LicenseDownloader.cs
│       │   ├── LicenseFileManager.cs
│       │   ├── PackageExclusionService.cs
│       │   └── LicenseDownloadOrchestrator.cs
│       ├── Program.cs
│       ├── appsettings.json
│       └── SBOM.Licenses.csproj
├── examples/
│   └── example-sbom.json
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

- [ ] Full SPDX format support
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
