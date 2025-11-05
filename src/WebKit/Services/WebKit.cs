using Microsoft.Extensions.Options;
using Talaryon.WebKit.Services.Options;

namespace Talaryon.WebKit.Services;

public class WebKit : IWebKit
{
    private readonly Dictionary<Type, object>
        _globalOptions = new();


    public WebKit(IOptions<WebKitSettings2> optionsAccessor)
    {
        ArgumentNullException.ThrowIfNull(optionsAccessor);
    }


    public void Configure<T>(Action<T> optionsConfigurator) where T : IWebKitOptions
    {
        if (_globalOptions.ContainsKey(typeof(T))) throw new WebKitOptionsAlreadyConfigured<T>();
        
        var options = Activator.CreateInstance<T>();
        optionsConfigurator(options);
        _globalOptions.Add(typeof(T), options);
    }

    public T GetOptions<T>() where T : IWebKitOptions
    {
        if(_globalOptions.TryGetValue(typeof(T), out var globalOptions))
            return (T)globalOptions;
        
        throw new WebKitOptionsNotConfigured<T>();
    }
}