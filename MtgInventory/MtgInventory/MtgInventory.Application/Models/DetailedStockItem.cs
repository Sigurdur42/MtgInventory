using System;
using MkmApi;

namespace MtgInventory.Service.Models
{
    public class DetailedStockItem : MkmStockItem
    {
        public DetailedStockItem(MkmStockItem item)
        {
            IdArticle = item.IdArticle;
            IdProduct = item.IdProduct;
            EnglishName = item.EnglishName;
            SetCode = item.SetCode;
            SetName = item.SetName;
            Price = item.Price;
            Language = item.Language;
            Condition = item.Condition;
            Foil = item.Foil;
            Signed = item.Signed;
            Playset = item.Playset;
            Altered = item.Altered;
            Comments = item.Comments;
            Amount = item.Amount;
            OnSale = item.OnSale;
        }

        public Guid ScryfallId { get; set; }
    }
}