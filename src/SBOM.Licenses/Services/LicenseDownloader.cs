using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SBOM.Licenses.Models;

namespace SBOM.Licenses.Services;

/// <summary>
/// Downloads license files from various sources (GitHub, direct URLs, NuGet packages)
/// </summary>
public class LicenseDownloader
{
    private readonly ILogger<LicenseDownloader> _logger;
    private readonly HttpClient _httpClient;
    private readonly GitHubLicenseService? _githubService;
    private static readonly string[] LicenseFileNames = new[]
    {
        "LICENSE", "LICENSE.txt", "LICENSE.md", "License.txt", "license.txt",
        "LICENCE", "LICENCE.txt", "LICENCE.md",
        "COPYING", "COPYING.txt",
        "LICENSE-MIT", "LICENSE-APACHE"
    };

    public LicenseDownloader(
        ILogger<LicenseDownloader> logger,
        HttpClient httpClient,
        GitHubLicenseService? githubService = null)
    {
        _logger = logger;
        _httpClient = httpClient;
        _githubService = githubService;
    }

    /// <summary>
    /// Downloads the license file for a component
    /// </summary>
    public async Task<LicenseDownloadResult> DownloadLicenseAsync(SbomComponent component)
    {
        _logger.LogInformation("Attempting to download license for {Component}", component);

        // Try different strategies to get the license

        // Strategy 1: GitHub API (if repository URL is available and GitHub service is configured)
        if (_githubService != null && !string.IsNullOrEmpty(component.RepositoryUrl))
        {
            var result = await TryDownloadFromGitHubAsync(component.RepositoryUrl, component);
            if (result.Success)
                return result;
        }

        // Strategy 2: Direct license URL
        if (!string.IsNullOrEmpty(component.LicenseUrl))
        {
            var result = await TryDownloadFromUrlAsync(component.LicenseUrl, component);
            if (result.Success)
                return result;
        }

        // Strategy 3: Extract from NuGet package
        var packageInfo = ParsePackageUrl(component.PackageUrl);
        if (packageInfo != null)
        {
            var result = await TryDownloadFromNuGetAsync(packageInfo.Value.Name, packageInfo.Value.Version, component);
            if (result.Success)
                return result;
        }

        // Strategy 4: Try NuGet with component name and version
        var nugetResult = await TryDownloadFromNuGetAsync(component.Name, component.Version, component);
        if (nugetResult.Success)
            return nugetResult;

        _logger.LogWarning("Could not download license for {Component}", component);
        return new LicenseDownloadResult
        {
            Success = false,
            Component = component,
            ErrorMessage = "No license file found"
        };
    }

    private async Task<LicenseDownloadResult> TryDownloadFromGitHubAsync(string repositoryUrl, SbomComponent component)
    {
        try
        {
            _logger.LogDebug("Trying to download license from GitHub: {RepositoryUrl}", repositoryUrl);

            var licenseResult = await _githubService!.GetLicenseAsync(repositoryUrl);
            if (licenseResult == null)
            {
                _logger.LogDebug("Could not retrieve license from GitHub for {RepositoryUrl}", repositoryUrl);
                return new LicenseDownloadResult { Success = false, Component = component };
            }

            _logger.LogInformation(
                "Successfully downloaded license from GitHub for {Component} (SPDX: {SpdxId})",
                component,
                licenseResult.SpdxId);

            return new LicenseDownloadResult
            {
                Success = true,
                Component = component,
                Content = licenseResult.Content,
                OriginalFileName = licenseResult.FileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error downloading from GitHub: {RepositoryUrl}", repositoryUrl);
            return new LicenseDownloadResult { Success = false, Component = component };
        }
    }

    private async Task<LicenseDownloadResult> TryDownloadFromUrlAsync(string url, SbomComponent component)
    {
        try
        {
            _logger.LogDebug("Trying to download license from URL: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Failed to download from URL {Url}: {StatusCode}", url, response.StatusCode);
                return new LicenseDownloadResult { Success = false, Component = component };
            }

            var content = await response.Content.ReadAsByteArrayAsync();
            var fileName = GetFileNameFromUrl(url);

            return new LicenseDownloadResult
            {
                Success = true,
                Component = component,
                Content = content,
                OriginalFileName = fileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error downloading from URL {Url}", url);
            return new LicenseDownloadResult { Success = false, Component = component };
        }
    }

    private async Task<LicenseDownloadResult> TryDownloadFromNuGetAsync(string packageName, string version, SbomComponent component)
    {
        try
        {
            _logger.LogDebug("Trying to download license from NuGet: {PackageName} {Version}", packageName, version);

            // Normalize version (remove any prefixes or suffixes)
            version = NormalizeVersion(version);

            // Download the .nupkg file from NuGet
            var nupkgUrl = $"https://api.nuget.org/v3-flatcontainer/{packageName.ToLowerInvariant()}/{version}/{packageName.ToLowerInvariant()}.{version}.nupkg";

            var response = await _httpClient.GetAsync(nupkgUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Failed to download NuGet package from {Url}: {StatusCode}", nupkgUrl, response.StatusCode);
                return new LicenseDownloadResult { Success = false, Component = component };
            }

            // NuGet packages are ZIP files
            using var nupkgStream = await response.Content.ReadAsStreamAsync();
            using var archive = new ZipArchive(nupkgStream, ZipArchiveMode.Read);

            // Look for license file in the package
            foreach (var fileName in LicenseFileNames)
            {
                var entry = archive.Entries.FirstOrDefault(e =>
                    e.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase) ||
                    e.FullName.EndsWith("/" + fileName, StringComparison.OrdinalIgnoreCase));

                if (entry != null)
                {
                    _logger.LogInformation("Found license file in NuGet package: {FileName}", entry.FullName);

                    using var entryStream = entry.Open();
                    using var memoryStream = new MemoryStream();
                    await entryStream.CopyToAsync(memoryStream);

                    return new LicenseDownloadResult
                    {
                        Success = true,
                        Component = component,
                        Content = memoryStream.ToArray(),
                        OriginalFileName = Path.GetFileName(entry.FullName)
                    };
                }
            }

            _logger.LogDebug("No license file found in NuGet package {PackageName} {Version}", packageName, version);
            return new LicenseDownloadResult { Success = false, Component = component };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error downloading from NuGet: {PackageName} {Version}", packageName, version);
            return new LicenseDownloadResult { Success = false, Component = component };
        }
    }

    private (string Name, string Version)? ParsePackageUrl(string? purl)
    {
        if (string.IsNullOrEmpty(purl))
            return null;

        try
        {
            // Parse package URL format: pkg:nuget/PackageName@Version
            if (purl.StartsWith("pkg:"))
            {
                var parts = purl.Split('/');
                if (parts.Length >= 2)
                {
                    var nameVersion = parts[^1].Split('@');
                    if (nameVersion.Length == 2)
                    {
                        return (nameVersion[0], nameVersion[1]);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error parsing package URL: {Purl}", purl);
        }

        return null;
    }

    private string GetFileNameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var fileName = Path.GetFileName(uri.LocalPath);
            return string.IsNullOrEmpty(fileName) ? "LICENSE" : fileName;
        }
        catch
        {
            return "LICENSE";
        }
    }

    private string NormalizeVersion(string version)
    {
        // Remove common prefixes
        version = version.TrimStart('v', 'V');

        // Remove build metadata (everything after '+')
        var plusIndex = version.IndexOf('+');
        if (plusIndex > 0)
        {
            version = version[..plusIndex];
        }

        return version;
    }
}

/// <summary>
/// Result of a license download attempt
/// </summary>
public class LicenseDownloadResult
{
    public bool Success { get; set; }
    public SbomComponent Component { get; set; } = null!;
    public byte[]? Content { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ErrorMessage { get; set; }
}
