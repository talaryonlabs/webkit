using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Talaryon.Toolbox.Services.Directus;
using Talaryon.WebKit.Models;

namespace Talaryon.WebKit.Services.WebKit;

public class WebKit : IWebKit
{
    private readonly IDirectus _directus;
    private readonly WebKitComponentCollection _components;
    
    public WebKit(IDirectus directus, IOptions<WebKitOptions> optionsAccessor)
    {
        _directus = directus;
        ArgumentNullException.ThrowIfNull(optionsAccessor);

        Default = optionsAccessor.Value;
        _components = optionsAccessor.Value.Components;
    }

    public WebKitOptions Default { get; }

    public Type? GetComponent<TBase>() where TBase : IWebKitComponent => _components.GetComponent<TBase>();

    public string? GetAssetUrl(string assetId) => _directus.GetAssetUrl(assetId);
    public string GetAssetUrl(string assetId, QueryString queryString) => _directus.GetAssetUrl(assetId, queryString);

    public async ValueTask<T?> Single<T>() where T : IDirectusModel
    {
        var item = Activator.CreateInstance<T>();
        var result = await _directus
            .Single<T>(item.GetTable())
            .Fields(item.GetFields())
            .RunAsync();

        return result != null ? result.Data ?? default : default;
    }

    public async ValueTask<T?> Select<T>(string? id) where T : IDirectusModel
    {
        var item = Activator.CreateInstance<T>();
        var result = await _directus
            .Select<T>(item.GetTable(), id)
            .Fields(item.GetFields())
            .RunAsync();

        return result != null ? result.Data ?? default : default;
    }

    public async ValueTask<DirectusResponse<T[]>?> Many<T>(int limit, int offset, string[] sort)
        where T : IDirectusModel
    {
        var item = Activator.CreateInstance<T>();
        var result = await _directus
            .Many<T>(item.GetTable())
            .Fields(item.GetFields())
            .Sort(sort)
            .Limit(limit)
            .Offset(offset)
            .IncludeMetadata()
            .RunAsync();

        return result ?? default;
    }

    public ValueTask<DirectusBlogPost?> GetBlogPost(string? id) => Select<DirectusBlogPost>(id);

    public Task<DirectusResponse<DirectusBlogPost[]>?> GetBlogPosts(int limit = 9, int offset = 0)
    {
        var item = Activator.CreateInstance<DirectusBlogPost>();
        
        return _directus
            .Many<DirectusBlogPost>(item.GetTable())
            .Fields(["id", "date_created", "date_scheduled", "title", "slug", "teaser", "image.id"])
            .Sort("-date_scheduled")
            .Limit(limit)
            .Offset(offset)
            .IncludeMetadata()
            .RunAsync();
    }
}