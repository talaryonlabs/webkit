namespace Talaryon.WebKit.Services.WebKit;

public class WebKitComponentCollection
{
    private readonly IDictionary<Type, Type> _components = new Dictionary<Type, Type>();
    
    public void SetComponent<TBase, TComponent>() where TBase : IWebKitComponent where TComponent : IWebKitComponent
    {
        if (_components.ContainsKey(typeof(TBase)))
            _components[typeof(TBase)] = typeof(TComponent);
        else
            _components.Add(typeof(TBase), typeof(TComponent));
    }

    public Type? GetComponent<TBase>() where TBase : IWebKitComponent
    {
        _components.TryGetValue(typeof(TBase), out var component);
        return component;
    }
}