namespace Talaryon.WebKit;

public interface IWebKitSession
{
    public List<WebKitComponent> Components { get; }
    public List<WebKitPage> Pages { get; }
    
    WebKitPage? ActivePage { get; set; }
}

public class WebKitSession : IWebKitSession
{
    public List<WebKitComponent> Components { get; } = [];
    public List<WebKitPage> Pages { get; } = [];
    public WebKitPage? ActivePage { get; set; }
}