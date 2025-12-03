using System.Text.Json.Serialization;

namespace SBOM.Licenses.Models.CycloneDx;

/// <summary>
/// CycloneDX SBOM format models
/// </summary>
public class CycloneDxBom
{
    [JsonPropertyName("bomFormat")]
    public string? BomFormat { get; set; }

    [JsonPropertyName("specVersion")]
    public string? SpecVersion { get; set; }

    [JsonPropertyName("components")]
    public List<CycloneDxComponent>? Components { get; set; }
}

public class CycloneDxComponent
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("purl")]
    public string? Purl { get; set; }

    [JsonPropertyName("licenses")]
    public List<CycloneDxLicense>? Licenses { get; set; }

    [JsonPropertyName("externalReferences")]
    public List<CycloneDxExternalReference>? ExternalReferences { get; set; }
}

public class CycloneDxLicense
{
    [JsonPropertyName("license")]
    public CycloneDxLicenseInfo? License { get; set; }

    [JsonPropertyName("expression")]
    public string? Expression { get; set; }
}

public class CycloneDxLicenseInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class CycloneDxExternalReference
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
