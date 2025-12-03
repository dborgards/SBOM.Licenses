# Versioning and Release Process

This project uses semantic versioning automatically calculated from git tags using [MinVer](https://github.com/adamralph/minver).

## Semantic Versioning

Version numbers follow the format: `MAJOR.MINOR.PATCH[-PRERELEASE]`

- **MAJOR**: Incremented for breaking changes
- **MINOR**: Incremented for new features (backward compatible)
- **PATCH**: Incremented for bug fixes (backward compatible)
- **PRERELEASE**: Optional identifier for pre-release versions

## How Versioning Works

MinVer automatically calculates the version based on:

1. **Git tags**: Tags prefixed with `v` (e.g., `v1.0.0`, `v2.1.0`)
2. **Commit history**: Distance from the last tag
3. **Branch status**: Whether the working tree is clean

### Version Calculation Examples

- On tag `v1.2.3`: Version is `1.2.3`
- 5 commits after `v1.2.3`: Version is `1.2.4-preview.0.5+<commit-hash>`
- No tags: Version is `0.0.0-preview.0.<height>+<commit-hash>`

## Creating a Release

To create a new release and publish to NuGet:

### 1. Create and Push a Version Tag

```bash
# For a stable release
git tag v1.0.0
git push origin v1.0.0

# For a pre-release
git tag v1.0.0-beta.1
git push origin v1.0.0-beta.1
```

### 2. Automatic Publishing

When you push a tag starting with `v`:

1. GitHub Actions builds the project
2. Creates a NuGet package with the version from the tag
3. Publishes to NuGet.org (if `NUGET_API_KEY` secret is configured)
4. Creates a GitHub Release with the package attached

## CI/CD Pipeline

The GitHub Actions workflow (`.github/workflows/build-and-publish.yml`) runs on:

- **Push to main**: Builds and tests the code
- **Pull requests**: Validates the build
- **Version tags** (`v*`): Builds, tests, and publishes to NuGet

### Workflow Steps

#### Build Job (runs on all triggers)
1. Checkout code with full history (required for MinVer)
2. Setup .NET 8.0
3. Restore dependencies
4. Build in Release configuration
5. Create NuGet package
6. Upload artifacts

#### Publish Job (runs only on version tags)
1. Download build artifacts
2. Publish to NuGet.org
3. Create GitHub Release

## Setup Requirements

### NuGet API Key

To enable automatic publishing to NuGet:

1. Create an API key at [nuget.org](https://www.nuget.org/account/apikeys)
2. Add it as a GitHub secret named `NUGET_API_KEY`:
   - Go to repository Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `NUGET_API_KEY`
   - Value: Your NuGet API key

### Permissions

The workflow requires:
- `contents: write` - For creating GitHub releases (automatically granted by `GITHUB_TOKEN`)

## Manual Build and Pack

To build and pack locally:

```bash
# Restore packages
dotnet restore src/SBOM.Licenses/SBOM.Licenses.csproj

# Build
dotnet build src/SBOM.Licenses/SBOM.Licenses.csproj --configuration Release

# Pack
dotnet pack src/SBOM.Licenses/SBOM.Licenses.csproj --configuration Release --output ./artifacts

# Check the version
dotnet msbuild src/SBOM.Licenses/SBOM.Licenses.csproj -getProperty:Version -nologo
```

## Version Tags Best Practices

1. **Use semantic versioning**: Follow the `MAJOR.MINOR.PATCH` format
2. **Tag stable releases**: Use clean version numbers (e.g., `v1.0.0`)
3. **Tag pre-releases**: Use identifiers like `-alpha`, `-beta`, `-rc` (e.g., `v1.0.0-beta.1`)
4. **Annotated tags**: Optionally use annotated tags for better git history

```bash
# Create an annotated tag
git tag -a v1.0.0 -m "Release version 1.0.0"
```

## Troubleshooting

### Version shows as `0.0.0-preview.0.X`

This happens when there are no tags in the repository. Create your first release tag:

```bash
git tag v1.0.0
git push origin v1.0.0
```

### Build fails with MinVer errors

Ensure:
- Git repository has full history (`fetch-depth: 0` in checkout action)
- Tags are pushed to the remote repository
- Tag format matches the prefix configuration (`v` prefix)

### NuGet publish fails

Check:
- `NUGET_API_KEY` secret is configured correctly
- API key has permission to push packages
- Package ID is not already taken on NuGet.org
- Version number is not a duplicate

## More Information

- [MinVer Documentation](https://github.com/adamralph/minver)
- [Semantic Versioning](https://semver.org/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [NuGet Package Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
