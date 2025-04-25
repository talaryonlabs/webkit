using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Talaryon.WebKit.Models;

public class DirectusFile
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    
    [DefaultValue(0)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width { get; set; }
    
    [DefaultValue(0)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Height { get; set; }
}