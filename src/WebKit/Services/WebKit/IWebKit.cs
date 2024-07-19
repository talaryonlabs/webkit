using Talaryon.Toolbox.Services.Directus;
using Talaryon.WebKit.Models;

namespace Talaryon.WebKit.Services.WebKit;

public interface IWebKit
{
    WebKitOptions Default { get; }

    ValueTask<DirectusBlogPost?> GetBlogPost(string id);
    Task<DirectusResponse<DirectusBlogPost[]>?> GetBlogPosts(int limit = 9, int offset = 0);
}