using Talaryon.WebKit.Models;

namespace Talaryon.WebKit.Components;

public class WebKitPriceViewItem
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public WebKitImage? Image { get; set; }
    public float Price { get; set; }
    public float OldPrice { get; set; }
    public string[] Including { get; set; } = [];
    public string[] Excluding { get; set; } = [];
}