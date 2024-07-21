using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Talaryon.WebKit.Models;

public class DirectusFooter : IDirectusModel
{
    public DirectusFooterLink[] Links { get; set; } = [];
    public DirectusFooterSocial[] Socials { get; set; } = [];
    public string? CopyrightText { get; set; }

    public virtual string GetTable() => "footer";
    public virtual string[] GetFields() =>
    [
        "copyrightText", "socials", "socials.sort", "socials.name", "socials.url", "socials.image.id", "links", "links.sort",
        "links.name", "links.url"
    ];
}

public class DirectusFooterSocial
{
    [DefaultValue(-1)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("sort")] 
    public int Sort { get; set; }

    public DirectusFile Image { get; set; } = new();
    public string? Name { get; set; }
    public string? Url { get; set; }
}

public class DirectusFooterLink
{
    [DefaultValue(-1)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("sort")] 
    public int Sort { get; set; }

    public string? Name { get; set; }
    public string? Url { get; set; }
}