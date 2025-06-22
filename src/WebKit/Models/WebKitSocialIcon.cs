namespace Talaryon.WebKit.Models;

public class WebKitSocialIcon : WebKitLink
{
    public WebKitSocialIcon()
    {
        Target = "_blank";
    }
    
    public string? Image { get; set; }
}