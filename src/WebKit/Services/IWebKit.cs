namespace Talaryon.WebKit.Services;

public interface IWebKit
{
    void ConfigureSimplePageMiddleware(Action<int, string> dynamicPageMiddlewareConfigurator);
    void Configure<T>(Action<T> optionsConfigurator, bool? force = false) where T : IWebKitOptions;
    T? GetOptions<T>() where T : IWebKitOptions;
    T? GetService<T>() where T : class;
}

public interface IWebKitOptions
{
    
}