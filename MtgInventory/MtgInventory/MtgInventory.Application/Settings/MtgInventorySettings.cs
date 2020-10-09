using System;
using MkmApi;

namespace MtgInventory.Service.Settings
{
    public class MtgInventorySettings
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public MkmAuthenticationData MkmAuthentication { get; set; } = new MkmAuthenticationData();

        public int RefreshPriceAfterDays { get; set; } = 5;
        
        public int RefreshSetDataAfterDays { get; set; } = 28;
    }
}