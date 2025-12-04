using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace SBOM.Licenses.Services;

/// <summary>
/// Service for retrieving license information from GitHub repositories
/// </summary>
public class GitHubLicenseService
{
    private readonly ILogger<GitHubLicenseService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string? _githubToken;

    public GitHubLicenseService(
        ILogger<GitHubLicenseService> logger,
        HttpClient httpClient,
        string? githubToken = null)
    {
        _logger = logger;
        _httpClient = httpClient;
        _githubToken = githubToken;

        // Set required User-Agent header for GitHub API
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SBOM-Licenses-Downloader");
        }

        // Add authorization header if token is provided
        if (!string.IsNullOrEmpty(_githubToken))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_githubToken}");
        }
    }

    /// <summary>
    /// Attempts to download license from a GitHub repository
    /// </summary>
    /// <param name="repositoryUrl">GitHub repository URL (e.g., https://github.com/owner/repo)</param>
    /// <returns>License content and metadata, or null if not found</returns>
    public async Task<GitHubLicenseResult?> GetLicenseAsync(string repositoryUrl)
    {
        var repoInfo = ParseGitHubUrl(repositoryUrl);
        if (repoInfo == null)
        {
            _logger.LogDebug("Not a valid GitHub URL: {Url}", repositoryUrl);
            return null;
        }

        try
        {
            var apiUrl = $"https://api.github.com/repos/{repoInfo.Value.Owner}/{repoInfo.Value.Repo}/license";
            _logger.LogDebug("Fetching license from GitHub API: {ApiUrl}", apiUrl);

            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug(
                    "GitHub API request failed for {Owner}/{Repo}: {StatusCode}",
                    repoInfo.Value.Owner,
                    repoInfo.Value.Repo,
                    response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var licenseResponse = JsonSerializer.Deserialize<GitHubLicenseResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (licenseResponse?.Content == null || licenseResponse.Encoding != "base64")
            {
                _logger.LogWarning("Unexpected GitHub API response format for {Owner}/{Repo}", repoInfo.Value.Owner, repoInfo.Value.Repo);
                return null;
            }

            // Decode base64 content
            var base64Content = licenseResponse.Content.Replace("\n", "").Replace("\r", "");
            var contentBytes = Convert.FromBase64String(base64Content);

            _logger.LogInformation(
                "Successfully retrieved license from GitHub: {Owner}/{Repo} (SPDX: {SpdxId})",
                repoInfo.Value.Owner,
                repoInfo.Value.Repo,
                licenseResponse.License?.SpdxId);

            return new GitHubLicenseResult
            {
                Content = contentBytes,
                FileName = licenseResponse.Name ?? "LICENSE",
                SpdxId = licenseResponse.License?.SpdxId,
                LicenseName = licenseResponse.License?.Name
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP error retrieving license from GitHub: {Owner}/{Repo}", repoInfo.Value.Owner, repoInfo.Value.Repo);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Error parsing GitHub API response for {Owner}/{Repo}", repoInfo.Value.Owner, repoInfo.Value.Repo);
            return null;
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Error decoding base64 license content for {Owner}/{Repo}", repoInfo.Value.Owner, repoInfo.Value.Repo);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving license from GitHub: {Owner}/{Repo}", repoInfo.Value.Owner, repoInfo.Value.Repo);
            return null;
        }
    }

    /// <summary>
    /// Parses a GitHub URL to extract owner and repository name
    /// </summary>
    private (string Owner, string Repo)? ParseGitHubUrl(string url)
    {
        try
        {
            // Handle various GitHub URL formats:
            // https://github.com/owner/repo
            // https://github.com/owner/repo.git
            // git@github.com:owner/repo.git
            // github.com/owner/repo

            url = url.Trim();

            // Remove git@ prefix for SSH URLs
            if (url.StartsWith("git@github.com:", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://github.com/" + url.Substring("git@github.com:".Length);
            }

            // Add https:// if missing
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            var uri = new Uri(url);

            // Check if it's a GitHub URL
            if (!uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Extract owner and repo from path
            var pathParts = uri.AbsolutePath.Trim('/').Split('/');
            if (pathParts.Length < 2)
            {
                return null;
            }

            var owner = pathParts[0];
            var repo = pathParts[1];

            // Remove .git suffix if present
            if (repo.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                repo = repo.Substring(0, repo.Length - 4);
            }

            return (owner, repo);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error parsing GitHub URL: {Url}", url);
            return null;
        }
    }
}

/// <summary>
/// Result of a GitHub license retrieval
/// </summary>
public class GitHubLicenseResult
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = "LICENSE";
    public string? SpdxId { get; set; }
    public string? LicenseName { get; set; }
}

/// <summary>
/// GitHub API license endpoint response model
/// </summary>
internal class GitHubLicenseResponse
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("encoding")]
    public string? Encoding { get; set; }

    [JsonPropertyName("license")]
    public GitHubLicenseInfo? License { get; set; }
}

/// <summary>
/// License information from GitHub API response
/// </summary>
internal class GitHubLicenseInfo
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("spdx_id")]
    public string? SpdxId { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
