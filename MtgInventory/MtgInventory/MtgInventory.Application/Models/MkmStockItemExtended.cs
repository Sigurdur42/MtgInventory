using System;
using System.Collections.Generic;
using System.Text;
using MkmApi;

namespace MtgInventory.Service.Models
{
   public  class MkmStockItemExtended 
    {
        public MkmStockItemExtended(MkmStockItem stockItem)
        {
            StockItem = stockItem;
        }

        public MkmStockItem StockItem { get;  }
    }
}
