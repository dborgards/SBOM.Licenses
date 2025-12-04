using System.Text.RegularExpressions;
using SBOM.Licenses.Configuration;

namespace SBOM.Licenses.Services;

/// <summary>
/// Service for checking if packages should be excluded based on configured patterns
/// </summary>
public class PackageExclusionService
{
    private readonly List<Regex> _exclusionRegexes;

    public PackageExclusionService(LicenseDownloaderConfig config)
    {
        _exclusionRegexes = config.ExcludedPackagePatterns
            .Select(ConvertWildcardToRegex)
            .ToList();
    }

    /// <summary>
    /// Checks if a package name matches any exclusion pattern
    /// </summary>
    /// <param name="packageName">The package name to check</param>
    /// <returns>True if the package should be excluded, false otherwise</returns>
    public bool IsExcluded(string packageName)
    {
        if (string.IsNullOrWhiteSpace(packageName))
        {
            return false;
        }

        return _exclusionRegexes.Any(regex => regex.IsMatch(packageName));
    }

    /// <summary>
    /// Converts a wildcard pattern (e.g., "Microsoft.*") to a regex pattern
    /// </summary>
    private static Regex ConvertWildcardToRegex(string wildcardPattern)
    {
        // Escape special regex characters except * and ?
        var regexPattern = Regex.Escape(wildcardPattern)
            .Replace("\\*", ".*")  // * matches any characters
            .Replace("\\?", ".");  // ? matches single character

        // Ensure exact match (anchor at start and end)
        regexPattern = $"^{regexPattern}$";

        return new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
