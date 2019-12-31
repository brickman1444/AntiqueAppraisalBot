using Xunit;

namespace AppraisalBot
{
    public static class TestPriceRange
    {
        [Theory]
        [InlineData(111, 230001, 100, 230000)]
        [InlineData(111, 222, 110, 220)]
        [InlineData(1, 5, 1, 5)]
        [InlineData(53962, 81960, 53000, 81000)]
        public static void PriceRangesAreRoundedToAppealingNumbers(int inLowPrice, int inHighPrice, int expectedRoundedLowPrice, int expectedRoundedHighPrice)
        {
            PriceRange inputRange = new PriceRange{
                lowPrice = inLowPrice,
                highPrice = inHighPrice,
            };

            PriceRange roundedRange = PriceRange.RoundPrices(inputRange);

            // Input range shouldn't be changed
            Assert.Equal(inLowPrice, inputRange.lowPrice);
            Assert.Equal(inHighPrice, inputRange.highPrice);

            Assert.Equal(expectedRoundedLowPrice, roundedRange.lowPrice);
            Assert.Equal(expectedRoundedHighPrice, roundedRange.highPrice);
        }
    }
}