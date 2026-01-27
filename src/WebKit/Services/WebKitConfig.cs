namespace Talaryon.WebKit.Services;

public interface IWebKitConfig
{
    void ConfigureOnce<T>(Action<T> optionsConfigurator) where T : IWebKitOptions;
    T Get<T>() where T : IWebKitOptions;
}

public class WebKitConfig(IWebKit webkit) : IWebKitConfig
{
    private readonly Dictionary<Type, object>
        _scopedOptions = new();
    
    public void ConfigureOnce<T>(Action<T> optionsConfigurator) where T : IWebKitOptions
    {
        Console.WriteLine($"Configuring scoped options for {typeof(T)}");
        
        // Drop scoped options first, if defined
        if (_scopedOptions.ContainsKey(typeof(T))) _scopedOptions.Remove(typeof(T));
        
        var options = Activator.CreateInstance<T>();
        optionsConfigurator(options);
        _scopedOptions.Add(typeof(T), options);
    }

    public T Get<T>() where T : IWebKitOptions
    {
        if (!_scopedOptions.TryGetValue(typeof(T), out var scopedOptions)) return webkit.GetOptions<T>();
        _scopedOptions.Remove(typeof(T));
        Console.WriteLine($"Reusing scoped options for {typeof(T)}");
        return (T)scopedOptions;
    }
}