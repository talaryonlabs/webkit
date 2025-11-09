using Microsoft.AspNetCore.Components;

namespace Talaryon.WebKit;

public class WebKitComponent  : ComponentBase, IDisposable
{
    private static readonly List<WebKitComponent> Components = [];

    public static void ApplyConfiguration()
    {
        lock (Components)
        {
            foreach (var c in Components)
            {
                c.OnConfigurationSet();
                c.StateHasChanged();
            }
        }
    }
    
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? InputAttributes { get; set; }

    public WebKitComponent()
    {
        lock (Components)
        {
            Components.Add(this);
        }
    }

    protected virtual void OnConfigurationSet()
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
    }
}