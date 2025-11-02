namespace Talaryon.WebKit.Services;

public interface IWebKit
{
    void ConfigureGlobal<T>(Action<T> optionsConfigurator) where T : IWebKitOptions;
    T GetOptions<T>() where T : IWebKitOptions;
    
    Type? GetComponent<TBase>() where TBase : IWebKitComponent;
}

public interface IWebKitOptions
{
    
}