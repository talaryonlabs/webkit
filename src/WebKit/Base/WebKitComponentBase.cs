using Microsoft.AspNetCore.Components;

namespace Talaryon.WebKit;

public class WebKitComponentBase<T>  : ComponentBase
    where T : IWebKitComponent
{
    protected Type? Component { get; set; } = typeof(T);
}