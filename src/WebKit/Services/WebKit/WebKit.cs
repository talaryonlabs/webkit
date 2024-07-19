using Microsoft.Extensions.Options;
using Talaryon.Toolbox;
using Talaryon.Toolbox.Services.Directus;
using Talaryon.WebKit.Models;

namespace Talaryon.WebKit.Services.WebKit;

public sealed class WebKitOptions : TalaryonOptions<WebKitOptions>
{
    public string? ApplicationName { get; set; }
    public string? DefaultMetaImage { get; set; }
    public string? DefaultPageTitle { get; set; }
    public string? DefaultPageDescription { get; set; }
    public string? TwitterAccount { get; set; }
}

public class WebKit : IWebKit
{
    private readonly IDirectus _directus;

    public WebKit(IDirectus directus, IOptions<WebKitOptions> optionsAccessor)
    {
        _directus = directus;
        ArgumentNullException.ThrowIfNull(optionsAccessor);

        Default = optionsAccessor.Value;
    }

    public WebKitOptions Default { get; }
    
    public async ValueTask<DirectusBlogPost?> GetBlogPost(string id)
    {
        var model = await _directus
            .Select<DirectusBlogPost>("blog", id)
            .Fields(["id", "date_scheduled", "title", "slug", "teaser", "content", "image.id", "user_created.first_name", "user_created.username", "user_created.avatar.id"])
            .RunAsync();
        
        return model?.Data;
    }

    public async Task<DirectusResponse<DirectusBlogPost[]>?> GetBlogPosts(int limit = 9, int offset = 0)
    {
        var response = await _directus
            .Many<DirectusBlogPost>("blog")
            .Fields(["id", "date_created", "date_scheduled", "title", "slug", "teaser", "image.id"])
            .Sort("-date_scheduled")
            .Limit(limit)
            .Offset(offset)
            .IncludeMetadata()
            .RunAsync();
        
        return response;
    }
}