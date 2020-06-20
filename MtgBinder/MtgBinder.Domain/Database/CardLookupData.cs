using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MtgBinder.Domain.Database
{
    public class CardLookupData
    {
        [Required]
        public string Lookup{get;set;}

        // TODO: enum for printing
    }
}
