namespace MtgJson.Sqlite.Models
{
    public class DbSet
    {
        public int BaseSetSize { get; set; }
        public string Code { get; set; } = "";
        public int? Id { get; set; }

        public bool IsFoilOnly { get; set; }
        public string MkmName { get; set; } = "";
        public string Name { get; set; } = "";
        public string ReleaseDate { get; set; } = "";
        public int TotalSetSize { get; set; }
    }
}