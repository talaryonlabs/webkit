﻿using Talaryon.WebKit.Models;

namespace Talaryon.WebKit.Components;

public class WebKitGridViewItem
{
    public required string? Title { get; set; }
    public required WebKitImage Image { get; set; } = new();
    public required string? Text { get; set; }
    public string? Subtitle { get; set; }
    public string? Url { get; set; }
}