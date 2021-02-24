using System;
using ScryfallApi.Client.Models;

namespace ScryfallApiServices.Models
{
    public class ScryfallSet
    {
        public ScryfallSet()
        {
        }

        public ScryfallSet(Set set)
        {
            Code = set.Code;
            MtgoCode = set.MtgoCode;
            Name = set.Name;
            SetType = set.SetType;
            ReleaseDate = set.ReleaseDate;
            BlockCode = set.BlockCode;
            Block = set.Block;
            ParentSetCode = set.ParentSetCode;
            card_count = set.card_count;
            IsDigital = set.IsDigital;
            IsFoilOnly = set.IsFoilOnly;
            IconSvgUri = set.IconSvgUri;
            SsearchUri = set.SsearchUri;

            UpdateDateUtc = DateTime.UtcNow;
        }

        public string Block { get; set; } = "";
        public string BlockCode { get; set; } = "";
        public int card_count { get; set; }
        public string Code { get; set; } = "";
        public Uri? IconSvgUri { get; set; }
        public Guid Id { get; set; }
        public bool IsDigital { get; set; }
        public bool IsFoilOnly { get; set; }
        public string MtgoCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string ParentSetCode { get; set; } = "";
        public DateTime? ReleaseDate { get; set; }
        public string SetType { get; set; } = "";
        public Uri? SsearchUri { get; set; }

        public DateTime? UpdateDateUtc { get; set; }
    }
}