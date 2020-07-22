namespace MtgBinder.Domain.Mkm
{
    public class MkmStockItem
    {
        public string IdArticle { get; set; }
        public string IdProduct { get; set; }
        public string EnglishName { get; set; }
        public string SetCode { get; set; }
        public string SetName { get; set; }

        public decimal Price { get; set; }
        public string Language { get; set; }
        public string Condition { get; set; }
        public bool Foil { get; set; }
        public bool Signed { get; set; }
        public bool Playset { get; set; }
        public bool Altered { get; set; }
        public string Comments { get; set; }
        public int Amount { get; set; }
        public bool OnSale { get; set; }

        //  "idArticle";"idProduct";"English Name";"Local Name";"Exp.";"Exp. Name";"Price";"Language";"Condition";"Foil?";"Signed?";"Playset?";"Altered?";"Comments";"Amount";"onSale"
    }
}