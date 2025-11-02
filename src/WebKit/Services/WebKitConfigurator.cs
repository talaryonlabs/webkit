namespace Talaryon.WebKit.Services;

public interface IWebKitConfigurator
{
    void ConfigureScoped<T>(Action<T> optionsConfigurator) where T : IWebKitOptions;
    void RegisterHook<T>(Action<T> hook) where T : IWebKitOptions;
    T Get<T>() where T : IWebKitOptions;
}

public class WebKitConfigurator(IWebKit webkit) : IWebKitConfigurator
{
    private readonly Dictionary<Type, object>
        _scopedOptions = new();
    
    private readonly Dictionary<Type, List<object>>
        _hooks = new();

    public void ConfigureScoped<T>(Action<T> optionsConfigurator) where T : IWebKitOptions
    {
        // Drop scoped options first, if defined
        if (_scopedOptions.ContainsKey(typeof(T))) _scopedOptions.Remove(typeof(T));
        
        var options = Activator.CreateInstance<T>();
        optionsConfigurator(options);
        _scopedOptions.Add(typeof(T), options);

        if (!_hooks.ContainsKey(typeof(T))) return;
        // options = Get<T>();
        foreach (var hook in _hooks[typeof(T)])
        {
            (hook as Action<T>)?.Invoke(options);
        }
    }

    public void RegisterHook<T>(Action<T> hook) where T : IWebKitOptions
    {
        if(!_hooks.ContainsKey(typeof(T))) _hooks.Add(typeof(T), []);
        _hooks[typeof(T)].Add(hook);
    }

    public T Get<T>() where T : IWebKitOptions
    {
        if (!_scopedOptions.TryGetValue(typeof(T), out var scopedOptions)) return webkit.GetOptions<T>();
        _scopedOptions.Remove(typeof(T));
        return (T)scopedOptions;
    }
}