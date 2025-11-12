using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Talaryon.WebKit.Services;

public class WebKit : IWebKit
{
    private readonly Dictionary<Type, object>
        _globalOptions = new();

    private WebApplication? _app;


    public WebKit(IOptions<WebKitSettings2> optionsAccessor)
    {
        ArgumentNullException.ThrowIfNull(optionsAccessor);
    }

    internal void UseApplication(WebApplication app)
    {
        _app = app;
    }

    public void ConfigureSimplePageMiddleware(Action<int, string> dynamicPageMiddlewareConfigurator)
    {
        
    }

    public void Configure<T>(Action<T> optionsConfigurator, bool? force = false) where T : IWebKitOptions
    {
        if (_globalOptions.ContainsKey(typeof(T)))
        {
            if (force == true) _globalOptions.Remove(typeof(T));
            else throw new WebKitOptionsAlreadyConfigured<T>();;
        }
        
        var options = Activator.CreateInstance<T>();
        optionsConfigurator(options);
        _globalOptions.Add(typeof(T), options);
    }

    public T? GetOptions<T>() where T : IWebKitOptions
    {
        if(_globalOptions.TryGetValue(typeof(T), out var globalOptions))
            return (T)globalOptions;

        return default;
    }

    public T? GetService<T>() where T : class
    {
        return _app?.Services.GetService<T>();
    }
}