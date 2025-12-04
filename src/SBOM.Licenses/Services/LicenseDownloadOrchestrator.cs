using Microsoft.Extensions.Logging;

namespace SBOM.Licenses.Services;

/// <summary>
/// Orchestrates the complete license download process
/// </summary>
public class LicenseDownloadOrchestrator
{
    private readonly ILogger<LicenseDownloadOrchestrator> _logger;
    private readonly SbomReader _sbomReader;
    private readonly LicenseDownloader _licenseDownloader;
    private readonly LicenseFileManager _fileManager;
    private readonly PackageExclusionService _exclusionService;

    public LicenseDownloadOrchestrator(
        ILogger<LicenseDownloadOrchestrator> logger,
        SbomReader sbomReader,
        LicenseDownloader licenseDownloader,
        LicenseFileManager fileManager,
        PackageExclusionService exclusionService)
    {
        _logger = logger;
        _sbomReader = sbomReader;
        _licenseDownloader = licenseDownloader;
        _fileManager = fileManager;
        _exclusionService = exclusionService;
    }

    /// <summary>
    /// Executes the complete license download workflow
    /// </summary>
    public async Task<DownloadSummary> ExecuteAsync(string sbomPath)
    {
        var summary = new DownloadSummary { SbomPath = sbomPath };

        try
        {
            _logger.LogInformation("=== Starting License Download Process ===");
            _logger.LogInformation("SBOM Path: {SbomPath}", sbomPath);

            // Step 1: Read SBOM
            _logger.LogInformation("Step 1: Reading SBOM...");
            var allComponents = await _sbomReader.ReadSbomAsync(sbomPath);
            summary.TotalComponents = allComponents.Count;

            if (allComponents.Count == 0)
            {
                _logger.LogWarning("No components found in SBOM");
                return summary;
            }

            _logger.LogInformation("Found {Count} components in SBOM", allComponents.Count);

            // Filter out excluded packages
            var excludedComponents = allComponents
                .Where(c => _exclusionService.IsExcluded(c.Name))
                .ToList();

            var components = allComponents
                .Where(c => !_exclusionService.IsExcluded(c.Name))
                .ToList();

            summary.ExcludedPackages = excludedComponents.Count;

            if (excludedComponents.Count > 0)
            {
                _logger.LogInformation("Excluded {Count} packages based on configured patterns:", excludedComponents.Count);
                foreach (var excluded in excludedComponents)
                {
                    _logger.LogDebug("  - {PackageName} (excluded)", excluded.Name);
                }
            }

            if (components.Count == 0)
            {
                _logger.LogWarning("All components were excluded - no licenses to download");
                return summary;
            }

            _logger.LogInformation("Processing {Count} components (after exclusions)", components.Count);

            // Step 2: Download licenses
            _logger.LogInformation("Step 2: Downloading licenses...");

            var downloadTasks = components.Select(async component =>
            {
                try
                {
                    var result = await _licenseDownloader.DownloadLicenseAsync(component);

                    if (result.Success)
                    {
                        // Step 3: Save license file
                        var savedPath = await _fileManager.SaveLicenseAsync(result);

                        if (savedPath != null)
                        {
                            return (Success: true, Component: component.Name, Error: (string?)null);
                        }
                        else
                        {
                            return (Success: false, Component: component.Name, Error: "Failed to save file");
                        }
                    }
                    else
                    {
                        return (Success: false, Component: component.Name, Error: result.ErrorMessage ?? "Download failed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing component {Component}", component.Name);
                    return (Success: false, Component: component.Name, Error: ex.Message);
                }
            });

            var results = await Task.WhenAll(downloadTasks);

            summary.SuccessfulDownloads = results.Count(r => r.Success);
            summary.FailedDownloads = results.Count(r => !r.Success);
            summary.FailedComponents = results.Where(r => !r.Success)
                .Select(r => $"{r.Component}: {r.Error}")
                .ToList();

            // Get final statistics
            var stats = _fileManager.GetStats();
            summary.TotalFilesCreated = stats.TotalFiles;
            summary.TotalSizeBytes = stats.TotalSizeBytes;
            summary.OutputDirectory = stats.OutputDirectory;

            _logger.LogInformation("=== Download Process Complete ===");
            _logger.LogInformation("Total components: {Total}", summary.TotalComponents);
            _logger.LogInformation("Excluded packages: {Excluded}", summary.ExcludedPackages);
            _logger.LogInformation("Successful downloads: {Success}", summary.SuccessfulDownloads);
            _logger.LogInformation("Failed downloads: {Failed}", summary.FailedDownloads);
            _logger.LogInformation("Total files: {Files}, Total size: {Size}",
                summary.TotalFilesCreated, stats.TotalSizeFormatted);

            if (summary.FailedComponents.Count > 0)
            {
                _logger.LogWarning("Failed components:");
                foreach (var failed in summary.FailedComponents)
                {
                    _logger.LogWarning("  - {Component}", failed);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during license download process");
            summary.FatalError = ex.Message;
        }

        return summary;
    }
}

/// <summary>
/// Summary of the download process
/// </summary>
public class DownloadSummary
{
    public string SbomPath { get; set; } = string.Empty;
    public int TotalComponents { get; set; }
    public int ExcludedPackages { get; set; }
    public int SuccessfulDownloads { get; set; }
    public int FailedDownloads { get; set; }
    public int TotalFilesCreated { get; set; }
    public long TotalSizeBytes { get; set; }
    public string OutputDirectory { get; set; } = string.Empty;
    public List<string> FailedComponents { get; set; } = new();
    public string? FatalError { get; set; }

    public bool HasErrors => FailedDownloads > 0 || FatalError != null;
}
