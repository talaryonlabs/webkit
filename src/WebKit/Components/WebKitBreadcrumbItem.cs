namespace Talaryon.WebKit.Components;

public class WebKitBreadcrumbItem
{
    public WebKitBreadcrumbItem()
    {
    }

    public WebKitBreadcrumbItem(string? name, string? url)
    {
        Name = name;
        Url = url;
    }

    public string? Name { get; set; }
    public string? Url { get; set; }
}