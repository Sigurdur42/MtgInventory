using System;
using System.Collections.Generic;
using System.Text;

namespace MtgInventory.Service.Models
{
    // TODO: Implement this like cards

    public class DetailedSetInfo
    {
        public Guid Id { get; set; }
        public string SetCode { get; set; }

        public string SetName { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
