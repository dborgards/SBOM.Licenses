using System.Text.Json.Serialization;

namespace SBOM.Licenses.Models.Spdx;

/// <summary>
/// SPDX SBOM format models (SPDX 2.3 specification)
/// </summary>
public class SpdxDocument
{
    [JsonPropertyName("spdxVersion")]
    public string? SpdxVersion { get; set; }

    [JsonPropertyName("dataLicense")]
    public string? DataLicense { get; set; }

    [JsonPropertyName("SPDXID")]
    public string? SpdxId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("documentNamespace")]
    public string? DocumentNamespace { get; set; }

    [JsonPropertyName("packages")]
    public List<SpdxPackage>? Packages { get; set; }
}

public class SpdxPackage
{
    [JsonPropertyName("SPDXID")]
    public string? SpdxId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("versionInfo")]
    public string? VersionInfo { get; set; }

    [JsonPropertyName("downloadLocation")]
    public string? DownloadLocation { get; set; }

    [JsonPropertyName("licenseConcluded")]
    public string? LicenseConcluded { get; set; }

    [JsonPropertyName("licenseDeclared")]
    public string? LicenseDeclared { get; set; }

    [JsonPropertyName("licenseInfoFromFiles")]
    public List<string>? LicenseInfoFromFiles { get; set; }

    [JsonPropertyName("copyrightText")]
    public string? CopyrightText { get; set; }

    [JsonPropertyName("externalRefs")]
    public List<SpdxExternalRef>? ExternalRefs { get; set; }
}

public class SpdxExternalRef
{
    [JsonPropertyName("referenceCategory")]
    public string? ReferenceCategory { get; set; }

    [JsonPropertyName("referenceType")]
    public string? ReferenceType { get; set; }

    [JsonPropertyName("referenceLocator")]
    public string? ReferenceLocator { get; set; }
}
