using System.Globalization;
using Talaryon.Toolbox;

namespace Talaryon.WebKit.Services.WebKit;

public class WebKitI18NOptions : IWebKitOptions
{
    public CultureInfo? CultureInfo { get; set; }
}