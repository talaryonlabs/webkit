using Talaryon.WebKit.Models;

namespace Talaryon.WebKit.Services.Options;

public class WebKitFooterOptions : IWebKitOptions
{
    public string? CopyrightText { get; set; }
    public List<WebKitLink> Links { get; set; } = new();
    public List<WebKitSocialIcon> SocialIcons { get; set; } = new();
}