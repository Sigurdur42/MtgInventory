using NUnit.Framework;

namespace MtgDatabase.Tests
{
    [TestFixture]
    public class MkmMapperTests
    {
        [TestCase("Signature Spellbook: Jace", "", "Signature-Spellbook-Jace")]
        [TestCase("War of the Spark", "", "War-of-the-Spark")]
        [TestCase("Magic Origins Tokens", "", "Magic-Origins")]
        [TestCase("Shadows over Innistrad Tokens", "", "Shadows-over-Innistrad")]      
        public void VerifyPatchSetName(string setName, string cardName, string expected)
        {
            var result = MkmMapper.PatchSetName(setName, cardName);
            Assert.AreEqual(expected, result);
        }
        
        [TestCase("", "Jace Beleren", "Jace-Beleren")]
        [TestCase("", "Jace, Arcane Strategist", "Jace-Arcane-Strategist")]  
        public void VerifyPatchCardName(string setName, string cardName, string expected)
        {
            var result = MkmMapper.PatchCardName(setName, cardName);
            Assert.AreEqual(expected, result);
        }
    }
}