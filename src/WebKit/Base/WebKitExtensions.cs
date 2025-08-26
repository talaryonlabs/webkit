using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Talaryon.Toolbox.Extensions;
using Talaryon.WebKit.Services;

namespace Talaryon.WebKit;

public static class WebKitExtensions
{
    public static IServiceCollection AddWebKitComponents(this IServiceCollection services, Action<WebKitSettings2>? optionsConfigurator = null)
    {
        if (optionsConfigurator is not null)
            services.AddSingleton<IWebKit, Services.WebKit, WebKitSettings2>(optionsConfigurator);
        else
            services.AddSingleton<IWebKit, Services.WebKit>();
        
        services.AddScoped<IWebKitNavigationManager, WebKitNavigationManager>();
        
        return services;
    }  

    public static void ConfigureGlobal<T>(this WebApplication app, Action<T> optionsConfigurator) where T : IWebKitOptions =>
        app
            .Services
            .GetService<IWebKit>()?
            .ConfigureGlobal(optionsConfigurator);
}