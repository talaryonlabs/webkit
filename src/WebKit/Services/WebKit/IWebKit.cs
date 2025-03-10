using Microsoft.AspNetCore.Http;
using Talaryon.Toolbox.Services.Directus;
using Talaryon.WebKit.Models;

namespace Talaryon.WebKit.Services.WebKit;

public interface IWebKit
{
    WebKitOptions Default { get; }
    
    Type? GetComponent<TBase>() where TBase : IWebKitComponent;
    
    string GetAssetUrl(string assetId);
    string GetAssetUrl(string assetId, QueryString queryString);
    
    ValueTask<T?> Single<T>() where T : IDirectusModel;
    ValueTask<T?> Select<T>(string? id) where T : IDirectusModel;
    ValueTask<DirectusResponse<T[]>?> Many<T>(int limit, int offset, string[] sort) where T : IDirectusModel;

    ValueTask<DirectusBlogPost?> GetBlogPost(string? id);
    Task<DirectusResponse<DirectusBlogPost[]>?> GetBlogPosts(int limit = 9, int offset = 0);

    ValueTask<DirectusPage?> GetPage(string? id) => Select<DirectusPage>(id);

    ValueTask<DirectusMeta?> GetMeta(string? id) => Select<DirectusMeta>(id);

    ValueTask<DirectusNavBar?> GetNavBar() => Single<DirectusNavBar>();

    ValueTask<DirectusFooter?> GetFooter() => Single<DirectusFooter>();
    

}