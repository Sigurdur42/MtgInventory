using System;

namespace MtgInventory.Service.Models
{
    public class SetUpdateInfo
    {
        public Guid Id { get; set; }
        public string SetCode { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}