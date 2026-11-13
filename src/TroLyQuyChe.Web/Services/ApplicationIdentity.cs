namespace TroLyQuyChe.Web.Services;

/// <summary>Presentation identity; leaves approved sources and stored citations intact.</summary>
public sealed class ApplicationIdentity
{
    public string FullName { get; set; } = "Mai Việt Hoàng";
    public string ShortName { get; set; } = "MVH";
    public string UpperName => FullName.ToUpperInvariant();
    public string OrganizationName => $"Công ty Phần mềm {FullName}";

    public string SourceLabel(string location)
    {
        // Normalize older internal source filenames without changing stored metadata.
        const string originalCollectionPrefix = "docs/nghiep-vu/02_QuyChe_GiaLap_";
        if (location.StartsWith(originalCollectionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var filenameEnd = location.IndexOf(".md", originalCollectionPrefix.Length, StringComparison.OrdinalIgnoreCase);
            if (filenameEnd >= 0)
                return $"Kho quy chế giả lập {FullName}{location[(filenameEnd + 3)..]}";
        }
        const string supplement = "docs/nghiep-vu/04_QuyChe_BoSung_Final.md";
        if (location.StartsWith(supplement, StringComparison.OrdinalIgnoreCase))
            return $"Kho quy chế bổ sung {FullName}{location[supplement.Length..]}";
        return location;
    }
}
