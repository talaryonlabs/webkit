using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Talaryon.Toolbox.Extensions;
using Talaryon.WebKit.Services;
using Talaryon.WebKit.Services.Options;

namespace Talaryon.WebKit;

public static class WebKitExtensions
{
    public static IServiceCollection AddWebKitComponents(this IServiceCollection services, Action<WebKitSettings2>? optionsConfigurator = null)
    {
        if (optionsConfigurator is not null)
            services.AddSingleton<IWebKit, Services.WebKit, WebKitSettings2>(optionsConfigurator);
        else
            services.AddSingleton<IWebKit, Services.WebKit>();
     
        services
            .AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
            
        services.AddHttpContextAccessor()
            .AddLocalization();
        
        services.AddScoped<IWebKitConfig, WebKitConfig>();
        services.AddScoped<IWebKitNavigation, WebKitNavigation>();
        
        return services;
    }
    
    public static WebApplication BuildWithWebKit<TRootComponent>(this WebApplicationBuilder builder, Action<IWebKit>? optionsConfigurator = null)
    {
        var app = builder.Build();
        var webkit = app.Services.GetService<IWebKit>() ?? throw new WebKitNotFound();

        (webkit as Talaryon.WebKit.Services.WebKit)!.UseApplication(app);
        
        
        // Configure the WebKit
        optionsConfigurator?.Invoke(webkit);
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
        }

        app.UseStatusCodePagesWithReExecute("/webkit/status-code/{0}");

        var i18NOptions = webkit.GetOptions<WebKitI18NOptions>();
        
        app.UseRequestLocalization(i18NOptions.RequestCultureProviders);
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<TRootComponent>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(WebKitExtensions).Assembly);

        return app;
    }

    public static void ConfigureGlobal<T>(this WebApplication app, Action<T> optionsConfigurator) where T : IWebKitOptions =>
        app
            .Services
            .GetService<IWebKit>()?
            .Configure(optionsConfigurator);
}