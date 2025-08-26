using Microsoft.AspNetCore.Components;
using Talaryon.Toolbox.Extensions;

namespace Talaryon.WebKit.Services;

public interface IWebKitNavigationManager
{
    void HandleError(int statusCode);
}

public class WebKitNavigationManager(IWebKit webKit, NavigationManager navigationManager) : IWebKitNavigationManager
{
    public void HandleError(int statusCode)
    {
        navigationManager.NavigateTo(
            $"/webkit/status-code/{statusCode}/{navigationManager.Uri.Replace(navigationManager.BaseUri, "/").ToBase64String()}"
        );
    }
}