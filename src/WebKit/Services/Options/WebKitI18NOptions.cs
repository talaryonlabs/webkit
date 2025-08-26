using System.Globalization;

namespace Talaryon.WebKit.Services.Options;

public class WebKitI18NOptions : IWebKitOptions
{
    public required CultureInfo CultureInfo { get; set; }
}