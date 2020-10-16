namespace MkmApi.Entities
{
    /// <summary>
    /// See https://api.cardmarket.com/ws/documentation/API_2.0:Entities:Article
    /// </summary>
    public class Article
    {
        /*
           article: 
        idArticle:                          // Article ID
        idProduct:                          // Product ID
        language: {                         // Language entity
            idLanguage:                     // Language ID
            languageName:                   // Language's name in English
        
        comments:                           // Comments
        price:                              // Price of the article
        count:                              // Count (see notes)
        inShoppingCart:                     // Flag, if that article is currently in a shopping cart
        product: {                          // Short Product entity
            enName:                         // English name
            locName:                        // Localized name (according to the language of the article)
            image:                          // path to the product's image
            expansion:                      // expansion name in English, if applicable
            nr:                             // Number of single within the expansion, if applicable
            expIcon:                        // index to the expansion's icon, if applicable
            rarity:                         // product's rarity if applicable
        
        seller: {}                          // Seller's user entity
        lastEdited:                         // Date, the article was last updated
        condition:                          // Product's condition, if applicable
        isFoil:                             // Foil flag, if applicable
        isSigned:                           // Signed flag, if applicable
        isAltered:                          // Altered flag, if applicable
        isPlayset:                          // Playset flaf, if applicable
        isFirstEd:                          // First edition flag, if applicable
        links: {}                           // HATEOAS links
    }
         * */

        public string IdArticle { get; set; } = "";
        public string IdProduct { get; set; } = "";

        public Language Language { get; set; } = new Language();
        public string Comments { get; set; } = "";
        public string Price { get; set; } = "";
        public string PriceEur { get; set; } = "";
        public string PriceGbp { get; set; } = "";
        public string Count { get; set; } = "";
        public string InShoppingCart { get; set; } = "";

        // TODO: Product

        public User Seller { get; set; } = new User();

        public string LastEdited { get; set; } = "";
        public string Condition { get; set; } = "";
        public string IsFoil { get; set; } = "";
        public string IsSigned { get; set; } = "";
        public string IsAltered { get; set; } = "";
        public string IsPlayset { get; set; } = "";
        public string IsFirstEdition { get; set; } = "";

        // TODO: Links
    }
}