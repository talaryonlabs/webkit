using Talaryon.Toolbox.Services.Directus;
using Talaryon.WebKit.Models;

namespace Talaryon.WebKit.Services.WebKit;

public interface IWebKit
{
    WebKitOptions Default { get; }

    ValueTask<T?> Single<T>(string table) where T : IDirectusModel;
    ValueTask<T?> Select<T>(string table, string id) where T : IDirectusModel;
    ValueTask<DirectusResponse<T[]>?> Many<T>(string table, int limit, int offset, string[] sort) where T : IDirectusModel;

    ValueTask<DirectusBlogPost?> GetBlogPost(string id);
    Task<DirectusResponse<DirectusBlogPost[]>?> GetBlogPosts(int limit = 9, int offset = 0);

    ValueTask<DirectusPage?> GetPage(string id);

    ValueTask<DirectusMeta?> GetMeta(string id);

    ValueTask<DirectusNavBar?> GetNavBar();

    ValueTask<DirectusFooter?> GetFooter() => GetFooter<DirectusFooter>();
    ValueTask<T?> GetFooter<T>() where T : DirectusFooter;
}