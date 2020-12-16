using System.Diagnostics;

namespace MkmApi.Entities
{
    /// <summary>
    /// See https://api.cardmarket.com/ws/documentation/API_2.0:Entities:Product
    /// </summary>
   [DebuggerDisplay("{IdProduct} {NameEn}")]
    public class Product
    {
        ////        /*

        ////        localization: {}            // localization entities for the product's name
        ////        category: {                 // Category entity the product belongs to
        ////            idCategory:             // Category ID
        ////            categoryName:           // Category's name
        ////        }
        ////
        ////        gameName:                   // the game's name
        ////        categoryName:               // the category's name
        ////        number:                     // Number of product within the expansion (where applicable)
        ////        rarity:                     // Rarity of product (where applicable)
        ////        expansionName:              // Expansion's name
        ////        links: {}                   // HATEOAS links
        ////        /* The following information is only returned for responses that return the detailed product entity */
        ////        expansion: {                // detailed expansion information (where applicable)
        ////            idExpansion:
        ////            enName:
        ////            expansionIcon:
        ////        }

        ////reprint:
        ////[                  // Reprint entities for each similar product bundled by the metaproduct
        ////            {
        ////idProduct:
        ////expansion:
        ////expIcon:
        ////            }
        ////        ]
        ////          */

        public string IdProduct { get; set; } = "";

        public string IdMetaproduct { get; set; } = "";

        public string NameEn { get; set; } = "";

        public string WebSite { get; set; } = "";
        public string Image { get; set; } = "";

        public int CountReprints { get; set; }

        public PriceGuide PriceGuide { get; set; } = new PriceGuide();

        public string OriginalResponse { get; set; } = "";
    }
}