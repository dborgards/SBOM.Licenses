namespace SBOM.Licenses.Configuration;

/// <summary>
/// Configuration for the license downloader application
/// </summary>
public class LicenseDownloaderConfig
{
    public const string SectionName = "LicenseDownloader";

    /// <summary>
    /// Output directory for downloaded license files
    /// </summary>
    public string OutputDirectory { get; set; } = "./licenses";

    /// <summary>
    /// Path to the SBOM file to process
    /// </summary>
    public string SbomPath { get; set; } = "./sbom.json";

    /// <summary>
    /// Whether to create the output directory if it doesn't exist
    /// </summary>
    public bool CreateOutputDirectoryIfNotExists { get; set; } = true;

    /// <summary>
    /// Whether to overwrite existing license files
    /// </summary>
    public bool OverwriteExistingFiles { get; set; } = false;

    /// <summary>
    /// Default file extension for license files without extension
    /// </summary>
    public string DefaultFileExtension { get; set; } = ".txt";

    /// <summary>
    /// List of package name patterns to exclude from license download.
    /// Supports wildcard patterns (e.g., "Microsoft.*", "System.*").
    /// Packages matching these patterns will be skipped.
    /// </summary>
    public List<string> ExcludedPackagePatterns { get; set; } = new();

    /// <summary>
    /// GitHub Personal Access Token for API requests.
    /// Optional but recommended to increase rate limits (5000/hour vs 60/hour).
    /// Can also be provided via environment variable GITHUB_TOKEN.
    /// </summary>
    public string? GitHubToken { get; set; }
}
