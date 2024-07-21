using System.Text.Json.Serialization;

namespace Talaryon.WebKit.Models;

public class DirectusPage : IDirectusModel
{
    [JsonPropertyName("date_created")] public DateTime DateCreated { get; set; }
    [JsonPropertyName("date_updated")] public DateTime DateUpdated { get; set; }

    public string? Title { get; set; }
    public string? Content { get; set; }

    public virtual string GetTable() => "pages";
    public virtual string[] GetFields() => ["id", "date_created", "date_updated", "title", "content"];
}