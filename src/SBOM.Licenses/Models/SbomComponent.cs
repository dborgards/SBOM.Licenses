namespace SBOM.Licenses.Models;

/// <summary>
/// Represents a component from the SBOM with its license information
/// </summary>
public class SbomComponent
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<string> Licenses { get; set; } = new();
    public string? LicenseUrl { get; set; }
    public string? PackageUrl { get; set; }
    public string? RepositoryUrl { get; set; }

    public override string ToString()
    {
        return $"{Name}@{Version} - Licenses: {string.Join(", ", Licenses)}";
    }
}
