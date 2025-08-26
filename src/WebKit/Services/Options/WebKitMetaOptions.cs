namespace Talaryon.WebKit.Services.Options;

public class WebKitMetaOptions : IWebKitOptions
{
    public string? ApplicationName { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string? BaseUrl { get; set; }
}