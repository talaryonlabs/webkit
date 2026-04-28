using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Talaryon.WebKit.Services;

namespace Talaryon.WebKit;

public class WebKitPage : ComponentBase, IDisposable
{
    [Inject] protected IWebKit? WebKit { get; init; }
    [Inject] protected IWebKitSession? WebKitSession { get; init; }
    [Inject] protected IHttpContextAccessor? HttpContextAccessor { get; init; }

    private string? CurcuitId => HttpContextAccessor?.HttpContext?.Connection.Id;
    
    public string? Title { get; protected set; }
    public string? Description { get; protected set; }
    public string? Image { get; protected set; }

    protected virtual void OnPageInitialized() { }
    
    protected override void OnInitialized()
    {
        if(WebKit is null) throw new InvalidOperationException("WebKit dependency is not injected");
        if(WebKitSession is null) throw new InvalidOperationException("WebKitSession dependency is not injected");
        if(HttpContextAccessor is null) throw new InvalidOperationException("HttpContextAccessor dependency is not injected");
        
        if(!WebKitSession.Pages.Contains(this))
            WebKitSession.Pages.Add(this);

        Console.WriteLine($"[CurcuitId {CurcuitId}] Initialize {GetType().Name}");
        OnPageInitialized();
        ApplyConfiguration();
    }


    protected override void OnParametersSet()
    {
        ApplyConfiguration();
    }

    protected void ApplyConfiguration()
    {
        var type = typeof(WebKitComponent);
        var method = type.GetMethod("OnPageChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        
        Console.WriteLine($"[CurcuitId {CurcuitId}] ApplyConfiguration");
        WebKitSession.Components
            .Where(v => !v.IsDisposed)
            .ToList()
            .ForEach(v =>
            {
                Console.WriteLine($"[CurcuitId {CurcuitId}] Invoke: {v.GetType().Name}");
                method.Invoke(v, [this]);
            });
    }

    public void Dispose()
    {
        if(WebKitSession.Pages.Contains(this))
            WebKitSession.Pages.Remove(this);
    }
}