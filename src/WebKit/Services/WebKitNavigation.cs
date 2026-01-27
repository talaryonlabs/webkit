using Microsoft.AspNetCore.Components;
using Talaryon.Toolbox.Extensions;

namespace Talaryon.WebKit.Services;

public interface IWebKitNavigation
{
    void HandleError(int statusCode);
}

public class WebKitNavigation : IWebKitNavigation
{
    private readonly NavigationManager _navigationManager;

    public WebKitNavigation(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }
    
    public void HandleError(int statusCode)
    {
        _navigationManager.NavigateTo(
            $"/webkit/status-code/{statusCode}/{_navigationManager.Uri.Replace(_navigationManager.BaseUri, "/").ToBase64String()}"
        );
    }
}