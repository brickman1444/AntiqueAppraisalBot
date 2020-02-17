
using Xunit;

namespace AppraisalBot
{
    public static class TestLanguageCodeToLocation
    {
        [Theory]
        [InlineData("en", "United States")]
        [InlineData("fr", "France")]
        [InlineData("sr-Cyrl", "Serbia")]
        [InlineData("unk", null)]
        [InlineData(null, null)]
        public static void LookUpReturnsUsableLocale(string languageCode, string locale)
        {
            Assert.Equal(locale, LanguageCodeToLocation.LookUp(languageCode));
        }
    }
}