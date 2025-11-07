namespace Talaryon.WebKit.Services;

public interface IWebKit
{
    void ConfigureSimplePageMiddleware(Action<int, string> dynamicPageMiddlewareConfigurator);
    void Configure<T>(Action<T> optionsConfigurator) where T : IWebKitOptions;
    T GetOptions<T>() where T : IWebKitOptions;
}

public interface IWebKitOptions
{
    
}