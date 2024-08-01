using Talaryon.WebKit.Models;

namespace Talaryon.WebKit.Components;

public interface IWebKitGalleryItem
{
    public DirectusFile Image { get; set; }
    public int Sort { get; set; }
}