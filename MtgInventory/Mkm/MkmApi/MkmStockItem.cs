using System.Diagnostics;
using CsvHelper.Configuration.Attributes;

namespace MkmApi
{
    [DebuggerDisplay("{EnglishName} {SetCode} {Price} {Language}")]
    public class MkmStockItem
    {
        [Name("idArticle")]
        public string IdArticle { get; set; }

        [Name("idProduct")]
        public string IdProduct { get; set; }

        [Name("English Name")]
        public string EnglishName { get; set; }

        [Name("Exp.")]
        public string SetCode { get; set; }

        [Name("Exp. Name")]
        public string SetName { get; set; }

        [Name("Price")]
        public decimal Price { get; set; }

        [Name("Language")]
        public string Language { get; set; }

        [Name("Condition")]
        public string Condition { get; set; }

        [Name("Foil?")]
        public string Foil { get; set; }

        [Name("Signed?")]
        public string Signed { get; set; }

        [Name("Playset?")]
        public string Playset { get; set; }

        [Name("Altered?")]
        public string Altered { get; set; }

        [Name("Comments")]
        public string Comments { get; set; }

        [Name("Amount")]
        public int Amount { get; set; }

        [Name("onSale")]
        public bool OnSale { get; set; }

        // ;"Price";"Language";"Condition";"Foil?";"Signed?";"Playset?";"Altered?";"Comments";"Amount";"onSale"
    }
}