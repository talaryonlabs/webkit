using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Talaryon.WebKit;

public static class WebKitExtensions
{
    public static IServiceCollection AddWebKitComponents(this IServiceCollection services) =>
        services
            .AddOptions()
            .AddSingleton<Services.WebKit.WebKit>()
            .AddSingleton<Services.WebKit.IWebKit>(x => x.GetRequiredService<Services.WebKit.WebKit>());

    public static void ConfigureGlobal<T>(this WebApplication app, Action<T> optionsConfigurator) where T : Services.WebKit.IWebKitOptions =>
        app
            .Services
            .GetService<Services.WebKit.IWebKit>()?
            .ConfigureGlobal<T>(optionsConfigurator);
}