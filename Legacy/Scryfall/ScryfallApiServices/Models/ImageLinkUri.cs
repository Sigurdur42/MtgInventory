namespace ScryfallApiServices.Models
{
    public class ImageLinkUri
    {
        public string Category { get; set; } = "";
        public string Uri { get; set; } = "";

        public bool IsValid => !string.IsNullOrWhiteSpace(Uri);
    }
}