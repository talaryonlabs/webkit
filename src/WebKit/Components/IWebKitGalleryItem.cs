using Talaryon.WebKit.Models;

namespace Talaryon.WebKit.Components;

public interface IWebKitGalleryItem
{
    public string? Title { get; set; }
    public string? Thumbnail { get; set; }
    public string? Image { get; set; }
    public int Sort { get; set; }
}