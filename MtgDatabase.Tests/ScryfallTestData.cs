using System.Collections.Generic;
using ScryfallApiServices.Models;

namespace MtgDatabase.Tests
{
    public class ScryfallTestData
    {
        // TODO: Deserialize test data
        public ScryfallTestData()
        {
            BasicLandSwamp = new ScryfallCard
            {
                Name = "Swamp",
                TypeLine = "Basic Land - Swamp",
                Legalities = new Dictionary<string, string>()
            };
        }

        public ScryfallCard BasicLandSwamp { get; }
    }
}