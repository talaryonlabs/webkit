namespace Talaryon.WebKit.Services.Options;

public class ContactComponent
{
    public class ContactOptions : IWebKitOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string MailFrom { get; set; } = string.Empty;
        public string MailTo { get; set; } = string.Empty;
    }
}
