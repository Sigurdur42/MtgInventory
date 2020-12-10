using System;

namespace MtgDatabase.Models
{
    public class SetInfo
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        
        public DateTime? ReleaseDate { get; set; }
        public bool IsDigital { get; set; }
        public string SetType { get; set; } = "";

        public string IconSvgUri { get; set; } = "";
        public int CardCount { get; set; }
    }
}