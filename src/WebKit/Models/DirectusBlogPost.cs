﻿using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Talaryon.WebKit.Models;

public class DirectusBlogPost : IDirectusModel
{
    public int Id { get; set; }
    [JsonPropertyName("date_scheduled")] public DateTime DateScheduled { get; set; }
    // [JsonPropertyName("user_created")] public SiteUser SiteUserCreated { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public DirectusFile Image { get; set; } = new();
    public string? Teaser { get; set; }
    public string? Content { get; set; }
    public DirectusBlogPostImage[] Gallery { get; set; } = [];
    
    public string GetTable() => "blog";
    public string[] GetFields() =>
    [
        "id", "date_scheduled", "title", "slug", "teaser", "content", "image.id", "user_created.first_name",
        "user_created.username", "user_created.avatar.id"
    ];
}

public class DirectusBlogPostImage
{
    [JsonPropertyName("directus_files_id")] 
    public string? Id { get; set; }
    
    [DefaultValue(-1)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("sort")]
    public int Sort { get; set; }
}