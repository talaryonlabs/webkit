using Talaryon.WebKit.Models;

namespace Talaryon.WebKit.Services.Options;

public class WebKitNavBarOptions : IWebKitOptions
{
    public string LogoUrl { get; set; }
    public List<WebKitLink> Links { get; set; }
    public string HeroImage { get; set; }
}