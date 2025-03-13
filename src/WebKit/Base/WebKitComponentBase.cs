using Microsoft.AspNetCore.Components;

namespace Talaryon.WebKit;

public abstract class WebKitComponentBase<T>  : ComponentBase
    where T : IWebKitComponent
{
    protected Type? Component { get; set; } = typeof(T);
    
    public abstract RenderFragment? ChildContent { get; set; }
    public abstract Dictionary<string, object>? InputAttributes { get; set; }
}