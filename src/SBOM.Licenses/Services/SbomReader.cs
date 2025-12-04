using System.Text.Json;
using Microsoft.Extensions.Logging;
using SBOM.Licenses.Models;
using SBOM.Licenses.Models.CycloneDx;

namespace SBOM.Licenses.Services;

/// <summary>
/// Reads and parses SBOM files in various formats
/// </summary>
public class SbomReader
{
    private readonly ILogger<SbomReader> _logger;

    public SbomReader(ILogger<SbomReader> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Reads an SBOM file and extracts component information
    /// </summary>
    public async Task<List<SbomComponent>> ReadSbomAsync(string sbomPath)
    {
        if (!File.Exists(sbomPath))
        {
            throw new FileNotFoundException($"SBOM file not found: {sbomPath}");
        }

        _logger.LogInformation("Reading SBOM file: {SbomPath}", sbomPath);

        var jsonContent = await File.ReadAllTextAsync(sbomPath);

        // Try to detect format by checking for specific properties
        if (jsonContent.Contains("\"bomFormat\"") || jsonContent.Contains("\"CycloneDX\""))
        {
            _logger.LogInformation("Detected CycloneDX format");
            return await ParseCycloneDxAsync(jsonContent);
        }
        else if (jsonContent.Contains("\"spdxVersion\"") || jsonContent.Contains("\"SPDX\""))
        {
            _logger.LogInformation("Detected SPDX format");
            return await ParseSpdxAsync(jsonContent);
        }
        else
        {
            throw new NotSupportedException("Unknown SBOM format. Supported formats: CycloneDX, SPDX");
        }
    }

    private async Task<List<SbomComponent>> ParseCycloneDxAsync(string jsonContent)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var bom = JsonSerializer.Deserialize<CycloneDxBom>(jsonContent, options);

        if (bom?.Components == null)
        {
            _logger.LogWarning("No components found in CycloneDX SBOM");
            return new List<SbomComponent>();
        }

        var components = new List<SbomComponent>();

        foreach (var component in bom.Components)
        {
            if (string.IsNullOrEmpty(component.Name))
                continue;

            var sbomComponent = new SbomComponent
            {
                Name = component.Name,
                Version = component.Version ?? "unknown",
                PackageUrl = component.Purl
            };

            // Extract license information
            if (component.Licenses != null)
            {
                foreach (var license in component.Licenses)
                {
                    if (license.License?.Id != null)
                    {
                        sbomComponent.Licenses.Add(license.License.Id);
                        if (license.License.Url != null)
                        {
                            sbomComponent.LicenseUrl = license.License.Url;
                        }
                    }
                    else if (license.Expression != null)
                    {
                        sbomComponent.Licenses.Add(license.Expression);
                    }
                }
            }

            // Look for license URL and repository URL in external references
            if (component.ExternalReferences != null)
            {
                var licenseRef = component.ExternalReferences
                    .FirstOrDefault(r => r.Type?.Equals("license", StringComparison.OrdinalIgnoreCase) == true);

                if (licenseRef?.Url != null)
                {
                    sbomComponent.LicenseUrl = licenseRef.Url;
                }

                // Extract VCS (repository) URL
                var vcsRef = component.ExternalReferences
                    .FirstOrDefault(r => r.Type?.Equals("vcs", StringComparison.OrdinalIgnoreCase) == true);

                if (vcsRef?.Url != null)
                {
                    sbomComponent.RepositoryUrl = vcsRef.Url;
                }
            }

            components.Add(sbomComponent);
        }

        _logger.LogInformation("Parsed {Count} components from CycloneDX SBOM", components.Count);
        return components;
    }

    private Task<List<SbomComponent>> ParseSpdxAsync(string jsonContent)
    {
        // SPDX parsing implementation
        // For now, return empty list with a warning
        _logger.LogWarning("SPDX format parsing not yet fully implemented");
        return Task.FromResult(new List<SbomComponent>());
    }
}
