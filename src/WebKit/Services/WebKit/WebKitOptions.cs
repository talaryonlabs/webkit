using System.Globalization;
using Talaryon.Toolbox;

namespace Talaryon.WebKit.Services.WebKit;

public sealed class WebKitOptions : TalaryonOptions<WebKitOptions>
{
    public string? ApplicationName { get; set; }
    public string? ApplicationUrl { get; set; }
    public string? DefaultMetaImage { get; set; }
    public string? DefaultPageTitle { get; set; }
    public string? DefaultPageDescription { get; set; }
    public CultureInfo DefaultCultureInfo { get; set; } = new("de-DE");
    public string? TwitterAccount { get; set; }
    public WebKitComponentTypes ComponentOverrides { get; } = new();
    
    public WebKitComponentCollection Components { get; } = new();
}

public sealed class WebKitComponentTypes
{
    public Type? Footer { get; set; }
    public Type? NavBar { get; set; }
}

public sealed class WebKitContainerSettings
{
    public string? MaxViewportSmall { get; set; }
}