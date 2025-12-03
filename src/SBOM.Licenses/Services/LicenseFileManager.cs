using Microsoft.Extensions.Logging;
using SBOM.Licenses.Models;

namespace SBOM.Licenses.Services;

/// <summary>
/// Manages saving license files with proper naming conventions
/// </summary>
public class LicenseFileManager
{
    private readonly ILogger<LicenseFileManager> _logger;
    private readonly string _outputDirectory;
    private readonly string _defaultExtension;
    private readonly bool _createDirectoryIfNotExists;
    private readonly bool _overwriteExisting;

    public LicenseFileManager(
        ILogger<LicenseFileManager> logger,
        string outputDirectory,
        string defaultExtension = ".txt",
        bool createDirectoryIfNotExists = true,
        bool overwriteExisting = false)
    {
        _logger = logger;
        _outputDirectory = outputDirectory;
        _defaultExtension = defaultExtension;
        _createDirectoryIfNotExists = createDirectoryIfNotExists;
        _overwriteExisting = overwriteExisting;
    }

    /// <summary>
    /// Saves a license file with proper naming: PackageName-Version.extension
    /// </summary>
    public async Task<string?> SaveLicenseAsync(LicenseDownloadResult downloadResult)
    {
        if (!downloadResult.Success || downloadResult.Content == null)
        {
            _logger.LogWarning("Cannot save license - download was not successful");
            return null;
        }

        try
        {
            // Ensure output directory exists
            if (_createDirectoryIfNotExists && !Directory.Exists(_outputDirectory))
            {
                Directory.CreateDirectory(_outputDirectory);
                _logger.LogInformation("Created output directory: {Directory}", _outputDirectory);
            }

            // Build file name: PackageName-Version.extension
            var fileName = BuildFileName(
                downloadResult.Component.Name,
                downloadResult.Component.Version,
                downloadResult.OriginalFileName);

            var filePath = Path.Combine(_outputDirectory, fileName);

            // Check if file already exists
            if (File.Exists(filePath) && !_overwriteExisting)
            {
                _logger.LogInformation("License file already exists, skipping: {FilePath}", filePath);
                return filePath;
            }

            // Save the file
            await File.WriteAllBytesAsync(filePath, downloadResult.Content);
            _logger.LogInformation("Saved license file: {FilePath}", filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving license file for {Component}", downloadResult.Component);
            return null;
        }
    }

    /// <summary>
    /// Builds the file name according to the pattern: PackageName-Version.extension
    /// </summary>
    private string BuildFileName(string packageName, string version, string? originalFileName)
    {
        // Sanitize package name and version for file system
        var sanitizedName = SanitizeFileName(packageName);
        var sanitizedVersion = SanitizeFileName(version);

        // Get extension from original file name, or use default
        var extension = GetFileExtension(originalFileName);

        // Build final file name
        return $"{sanitizedName}-{sanitizedVersion}{extension}";
    }

    /// <summary>
    /// Gets the file extension from the original file name, or returns default
    /// </summary>
    private string GetFileExtension(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return _defaultExtension;
        }

        var extension = Path.GetExtension(fileName);

        // If no extension, use default
        if (string.IsNullOrEmpty(extension))
        {
            return _defaultExtension;
        }

        // Ensure extension starts with a dot
        if (!extension.StartsWith('.'))
        {
            extension = "." + extension;
        }

        return extension;
    }

    /// <summary>
    /// Sanitizes a string to be safe for use as a file name
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        // Replace invalid characters with underscore
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Select(c =>
            invalidChars.Contains(c) ? '_' : c).ToArray());

        // Remove any leading/trailing dots or spaces
        sanitized = sanitized.Trim('.', ' ');

        return sanitized;
    }

    /// <summary>
    /// Gets statistics about saved licenses
    /// </summary>
    public LicenseFileStats GetStats()
    {
        if (!Directory.Exists(_outputDirectory))
        {
            return new LicenseFileStats();
        }

        var files = Directory.GetFiles(_outputDirectory);

        return new LicenseFileStats
        {
            TotalFiles = files.Length,
            TotalSizeBytes = files.Sum(f => new FileInfo(f).Length),
            OutputDirectory = _outputDirectory
        };
    }
}

/// <summary>
/// Statistics about saved license files
/// </summary>
public class LicenseFileStats
{
    public int TotalFiles { get; set; }
    public long TotalSizeBytes { get; set; }
    public string OutputDirectory { get; set; } = string.Empty;

    public string TotalSizeFormatted => FormatBytes(TotalSizeBytes);

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
