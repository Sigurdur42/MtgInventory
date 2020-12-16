namespace MtgDatabase.Models
{
    public class CardImages
    {
        public string Normal { get; set; } = "";
        public string Large { get; set; } = "";
        public string Small { get; set; } = "";
        public string Png { get; set; } = "";
        public string ArtCrop { get; set; } = "";
        public string BorderCrop { get; set; } = "";

        // TODO: Shorten uri (fix + category)
    }
}