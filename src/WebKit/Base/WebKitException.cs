namespace Talaryon.WebKit;

public class WebKitException : Exception
{
    public WebKitException(string message)
        : base(message)
    {
        Console.WriteLine(message);
    }
}

public class WebKitComponentNotFoundException() : Exception("WebKitComponent not found.");

public class WebKitOptionsNotConfigured<T>() : Exception($"Options {typeof(T).Name} not configured.");
public class WebKitOptionsAlreadyConfigured<T>() : Exception($"Options {typeof(T).Name} already configured globally. Use ConfigureScoped to override one time.");