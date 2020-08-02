using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ScryfallApi.Client.Models;

namespace MtgBinder.Domain.Database
{
    public class CardLookupData
    {
        [Required]
        public string Lookup{get;set;}

        public SearchOptions.RollupMode Mode { get; set; } = SearchOptions.RollupMode.Cards;
        // TODO: enum for printing
    }
}
