using Microsoft.Extensions.Options;

namespace Talaryon.WebKit.Services.WebKit;

public class WebKit : IWebKit
{
    private readonly WebKitComponentCollection _components;
    private readonly Dictionary<Type, object> 
        _globalOptions = new(),
        _scopedOptions = new();
    
    public WebKit(IOptions<WebKitOptions> optionsAccessor)
    {
        ArgumentNullException.ThrowIfNull(optionsAccessor);

        Default = optionsAccessor.Value;
        _components = optionsAccessor.Value.Components;
    }

    public WebKitOptions Default { get; }

    public void ConfigureGlobal<T>(Action<T> optionsConfigurator) where T : IWebKitOptions
    {
        if (_globalOptions.ContainsKey(typeof(T))) throw new WebKitOptionsAlreadyConfigured<T>();
        
        var options = Activator.CreateInstance<T>();
        optionsConfigurator(options);
        _globalOptions.Add(typeof(T), options);
    }

    public void ConfigureScoped<T>(Action<T> optionsConfigurator) where T : IWebKitOptions
    {
        // Drop scoped options first, if defined
        if (_scopedOptions.ContainsKey(typeof(T))) _scopedOptions.Remove(typeof(T));
        
        var options = Activator.CreateInstance<T>();
        optionsConfigurator(options);
        _scopedOptions.Add(typeof(T), options);   
    }

    public T GetOptions<T>() where T : IWebKitOptions
    {
        if (_scopedOptions.TryGetValue(typeof(T), out var scopedOptions))
        {
            _scopedOptions.Remove(typeof(T));
            return (T)scopedOptions;
        }
        
        if(_globalOptions.TryGetValue(typeof(T), out var globalOptions))
            return (T)globalOptions;
        
        throw new WebKitOptionsNotConfigured<T>();
    }

    public Type? GetComponent<TBase>() where TBase : IWebKitComponent => _components.GetComponent<TBase>();
}