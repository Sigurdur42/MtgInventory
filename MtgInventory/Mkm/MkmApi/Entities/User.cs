namespace MkmApi.Entities
{
    /// <summary>
    /// See https://api.cardmarket.com/ws/documentation/API_2.0:Entities:User for details
    /// </summary>
    public class User
    {
        /*
         *  idUser:                     // user ID
        username:                   // username
        registrationDate:           // date of registration
        isCommercial:               // 0: private user
                                    // 1: commercial user
                                    // 2: powerseller
        isSeller:                   // indicates if the user can sell; true|false
        name: {                     // name entity
            company:                // company name; only returned for commercial users
            firstName:              // first name
            lastName:               // last name; only returned for commercial users
        
        address: {}                 // Address entity
        phone:                      // phone number; only returned for commercial users
        email:                      // email address; only returned for commercial users
        vat:                        // tax number; only returned for commercial users
        riskGroup:                  // 0: no risk
                                    // 1: low risk
                                    // 2: high risk
        reputation:                 // 0: not enough sells to rate
                                    // 1: outstanding seller
                                    // 2: very good seller
                                    // 3: good seller
                                    // 4: average seller
                                    // 5: bad seller
        shipsFast:                  // Expected amount of days it will take to receive an order placed from this seller
        sellCount:                  // number of sales
        soldItems:                  // total number of sold items
        avgShippingTime:            // average shipping time
        onVacation:                 // true|false
        links: []                   // HATEOAS links
         * */

        public string IdUser { get; set; }
        public string UserName { get; set; }
        public string RegistrationDate { get; set; }

        /// <summary>
        /// 0: private user
        /// 1: commercial user
        /// 2: powerseller
        /// </summary>
        public int IsCommercial { get; set; }

        public string IsSeller { get; set; }

        // TODO: Name
        // TODO: Address

        public string Phone { get; set; }
        public string Email { get; set; }
        public string Vat { get; set; }

        public int RiskGroup { get; set; }
        public int Reputation { get; set; }
        public string ShipsFast { get; set; }
        public string SellCount { get; set; }
        public int SoldItems { get; set; }
        public string AvgShippingTime { get; set; }
        public string OnVacation { get; set; }

        public string LegalInformation { get; set; }
        public string LossPercentage { get; set; }

        public string UnsentShipments { get; set; }
        // TODO: Links
    }
}