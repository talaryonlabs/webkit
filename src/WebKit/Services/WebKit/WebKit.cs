using Microsoft.Extensions.Options;
using Talaryon.Toolbox;
using Talaryon.Toolbox.Services.Directus;
using Talaryon.WebKit.Models;

namespace Talaryon.WebKit.Services.WebKit;

public sealed class WebKitOptions : TalaryonOptions<WebKitOptions>
{
    public string? ApplicationName { get; set; }
    public string? ApplicationUrl { get; set; }
    public string? DefaultMetaImage { get; set; }
    public string? DefaultPageTitle { get; set; }
    public string? DefaultPageDescription { get; set; }
    public string? TwitterAccount { get; set; }

    public WebKitOptionsTables Tables { get; } = new();
}

public sealed class WebKitOptionsTables
{
    public string Blog { get; set; } = "blog";
    public string Page { get; set; } = "pages";
    public string NavBar { get; set; } = "navbar";
    public string Meta { get; set; } = "meta";

    public string Footer { get; set; } = "footer";
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

    public async ValueTask<T?> Single<T>(string table) where T : IDirectusModel
    {
        var item = Activator.CreateInstance<T>();
        var result = await _directus
            .Single<T>(table)
            .Fields(item.GetFields())
            .RunAsync();

        return result != null ? result.Data ?? default : default;
    }

    public async ValueTask<T?> Select<T>(string table, string id) where T : IDirectusModel
    {
        var item = Activator.CreateInstance<T>();
        var result = await _directus
            .Select<T>(table, id)
            .Fields(item.GetFields())
            .RunAsync();

        return result != null ? result.Data ?? default : default;
    }

    public async ValueTask<DirectusResponse<T[]>?> Many<T>(string table, int limit, int offset, string[] sort)
        where T : IDirectusModel
    {
        var item = Activator.CreateInstance<T>();
        var result = await _directus
            .Many<T>(table)
            .Fields(item.GetFields())
            .Sort(sort)
            .Limit(limit)
            .Offset(offset)
            .IncludeMetadata()
            .RunAsync();

        return result ?? default;
    }

    public async ValueTask<DirectusBlogPost?> GetBlogPost(string id)
    {
        var model = await _directus
            .Select<DirectusBlogPost>(Default.Tables.Blog, id)
            .Fields([
                "id", "date_scheduled", "title", "slug", "teaser", "content", "image.id", "user_created.first_name",
                "user_created.username", "user_created.avatar.id"
            ])
            .RunAsync();

        return model?.Data;
    }

    public async Task<DirectusResponse<DirectusBlogPost[]>?> GetBlogPosts(int limit = 9, int offset = 0)
    {
        var response = await _directus
            .Many<DirectusBlogPost>(Default.Tables.Blog)
            .Fields(["id", "date_created", "date_scheduled", "title", "slug", "teaser", "image.id"])
            .Sort("-date_scheduled")
            .Limit(limit)
            .Offset(offset)
            .IncludeMetadata()
            .RunAsync();

        return response;
    }

    public async ValueTask<DirectusPage?> GetPage(string id)
    {
        return await Select<DirectusPage>(
            table: Default.Tables.Page,
            id: id
        );
    }

    public async ValueTask<DirectusMeta?> GetMeta(string id)
    {
        return await Select<DirectusMeta>(
            table: Default.Tables.Meta,
            id: id
        );
    }

    public async ValueTask<DirectusNavBar?> GetNavBar() =>
        await Single<DirectusNavBar>(
            table: Default.Tables.NavBar
        );

    public async ValueTask<T?> GetFooter<T>() where T : DirectusFooter =>
        await Single<T>(
            table: Default.Tables.Footer
        );
}