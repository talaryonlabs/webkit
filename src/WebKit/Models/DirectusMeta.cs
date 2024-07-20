namespace Talaryon.WebKit.Models;

public class DirectusMeta : IDirectusModel
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DirectusFile? Image { get; set; }
    public string[] GetFields() => ["*.*"];
}