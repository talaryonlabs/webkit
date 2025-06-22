using Talaryon.Toolbox;

namespace Talaryon.WebKit.Services.WebKit;

public class WebKitMetaOptions : IWebKitOptions
{
    public string? ApplicationName { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
}