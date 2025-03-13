using Microsoft.AspNetCore.Components;

namespace Talaryon.WebKit;

public interface IWebKitComponent
{
    public RenderFragment? ChildContent { get; set; }
    public Dictionary<string, object>? InputAttributes { get; set; }
}