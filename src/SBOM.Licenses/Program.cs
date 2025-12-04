using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SBOM.Licenses.Configuration;
using SBOM.Licenses.Services;

namespace SBOM.Licenses;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddCommandLine(args)
            .Build();

        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConfiguration(configuration.GetSection("Logging"))
                .AddConsole();
        });

        var logger = loggerFactory.CreateLogger<Program>();

        try
        {
            logger.LogInformation("SBOM License Downloader v1.0");
            logger.LogInformation("=====================================");

            // Load configuration
            var config = new LicenseDownloaderConfig();
            configuration.GetSection(LicenseDownloaderConfig.SectionName).Bind(config);

            // Override with command line arguments if provided
            if (args.Length > 0)
            {
                config.SbomPath = args[0];
            }
            if (args.Length > 1)
            {
                config.OutputDirectory = args[1];
            }

            // Validate configuration
            if (!File.Exists(config.SbomPath))
            {
                logger.LogError("SBOM file not found: {SbomPath}", config.SbomPath);
                logger.LogInformation("");
                logger.LogInformation("Usage:");
                logger.LogInformation("  SBOM.Licenses [sbom-path] [output-directory]");
                logger.LogInformation("");
                logger.LogInformation("Examples:");
                logger.LogInformation("  SBOM.Licenses ./sbom.json ./licenses");
                logger.LogInformation("  SBOM.Licenses ./my-project-sbom.json");
                logger.LogInformation("");
                logger.LogInformation("Configuration can also be set in appsettings.json");
                return 1;
            }

            logger.LogInformation("Configuration:");
            logger.LogInformation("  SBOM Path: {SbomPath}", config.SbomPath);
            logger.LogInformation("  Output Directory: {OutputDirectory}", config.OutputDirectory);
            logger.LogInformation("  Default Extension: {Extension}", config.DefaultFileExtension);
            logger.LogInformation("  Overwrite Existing: {Overwrite}", config.OverwriteExistingFiles);
            if (config.ExcludedPackagePatterns.Count > 0)
            {
                logger.LogInformation("  Excluded Patterns: {Patterns}", string.Join(", ", config.ExcludedPackagePatterns));
            }
            logger.LogInformation("");

            // Create services
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "SBOM-License-Downloader/1.0");

            var sbomReader = new SbomReader(loggerFactory.CreateLogger<SbomReader>());
            var licenseDownloader = new LicenseDownloader(
                loggerFactory.CreateLogger<LicenseDownloader>(),
                httpClient);
            var fileManager = new LicenseFileManager(
                loggerFactory.CreateLogger<LicenseFileManager>(),
                config.OutputDirectory,
                config.DefaultFileExtension,
                config.CreateOutputDirectoryIfNotExists,
                config.OverwriteExistingFiles);
            var exclusionService = new PackageExclusionService(config);
            var orchestrator = new LicenseDownloadOrchestrator(
                loggerFactory.CreateLogger<LicenseDownloadOrchestrator>(),
                sbomReader,
                licenseDownloader,
                fileManager,
                exclusionService);

            // Execute
            var summary = await orchestrator.ExecuteAsync(config.SbomPath);

            // Display summary
            logger.LogInformation("");
            logger.LogInformation("=====================================");
            logger.LogInformation("Summary:");
            logger.LogInformation("  Total Components: {Total}", summary.TotalComponents);
            logger.LogInformation("  Excluded Packages: {Excluded}", summary.ExcludedPackages);
            logger.LogInformation("  Successful Downloads: {Success}", summary.SuccessfulDownloads);
            logger.LogInformation("  Failed Downloads: {Failed}", summary.FailedDownloads);
            logger.LogInformation("  Output Directory: {Directory}", summary.OutputDirectory);
            logger.LogInformation("=====================================");

            if (summary.HasErrors)
            {
                logger.LogWarning("Process completed with errors");
                return 1;
            }

            logger.LogInformation("Process completed successfully!");
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error occurred");
            return 1;
        }
    }
}
