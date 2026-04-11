using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Talaryon.WebKit.Services;

namespace Talaryon.WebKit;

public class WebKitComponent : ComponentBase, IDisposable
{
    public bool IsDisposed { get; private set; }
    
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? InputAttributes { get; set; }

    [Inject] protected IWebKit? WebKit { get; init; }
    [Inject] protected IWebKitSession? WebKitSession { get; init; }
    [Inject] protected IHttpContextAccessor? HttpContextAccessor { get; init; }

    private string? CurcuitId => HttpContextAccessor?.HttpContext?.Connection.Id;

    protected virtual void OnPageChanged(WebKitPage page)
    {
    }

    protected virtual void OnComponentInitialized()
    {
    }

    protected override void OnInitialized()
    {
        if (!WebKitSession.Components.Contains(this))
            WebKitSession.Components.Add(this);
        
        Console.WriteLine($"[CurcuitId {CurcuitId}] Initialize {GetType().Name}");
        OnComponentInitialized();
        
    }

    protected string? GetWebKitClass()
    {
        return (string?)InputAttributes?.GetValueOrDefault("webkit:class", "");
    }

    protected string? GetWebKitStyle()
    {
        return (string?)InputAttributes?.GetValueOrDefault("webkit:style", "");
    }

    public void Dispose()
    {
        IsDisposed = true;
        if (WebKitSession.Components.Contains(this))
            WebKitSession.Components.Remove(this);
        
        Console.WriteLine($"[CurcuitId {CurcuitId}] Dispose {GetType().Name}");
    }
}