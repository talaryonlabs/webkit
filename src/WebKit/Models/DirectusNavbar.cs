using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Talaryon.WebKit.Models;

public class DirectusNavbar : IDirectusModel
{ 
    public string? Title { get; set; }
    public DirectusFile Logo { get; set; } = new();
    public DirectusNavbarLink[] Links { get; set; } = [];
    
    public virtual string GetTable() => "navbar";
    public virtual string[] GetFields() => ["title", "logo", "logo.id", "links", "links.sort", "links.name", "links.url"];
}


public class DirectusNavbarLink
{
    [DefaultValue(-1)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("sort")] 
    public int Sort { get; set; }

    public required string Name { get; set; }
    public required string Url { get; set; }
}