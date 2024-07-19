using System.Text.Json.Serialization;

namespace Talaryon.WebKit.Models;

public class DirectusUser
{
    [JsonPropertyName("first_name")] public string FirstName { get; set; }
    public DirectusFile Avatar { get; set; }
    public string Username { get; set; }
    // public string Bio { get; set; }
}