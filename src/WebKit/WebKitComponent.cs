using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Talaryon.WebKit;

public class WebKitComponent  : ComponentBase, IDisposable
{
    private static readonly List<WebKitComponent> Components = [];
    
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? InputAttributes { get; set; }

    [Inject]
    private NavigationManager? NavigationManager
    {
        get;
        set
        {
            if ((field = value) is not null)
            {
                field.LocationChanged += OnLocationChanged;
            }
        }
    }

    public WebKitComponent()
    {
        lock (Components)
        {
            Components.Add(this);
        }
    }

    protected virtual void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        
    }

    protected string? GetWebKitClass()
    {
        return (string?)InputAttributes?.GetValueOrDefault("webkit:class", "");
    }
    
    protected string? GetWebKitStyle()
    {
        return (string?)InputAttributes?.GetValueOrDefault("webkit:style", "");
    }

    void IDisposable.Dispose()
    {
        lock (Components)
        {
            if (Components.Contains(this)) Components.Remove(this);
        }
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}