using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AppraisalBot
{
    public static class TestPriceRange
    {
        [Theory]
        [InlineData(111, 230001, 100, 230000)]
        [InlineData(111, 222, 110, 220)]
        [InlineData(1, 5, 1, 5)]
        public static void PriceRangesAreRoundedToAppealingNumbers(int inLowPrice, int inHighPrice, int outLowPrice, int outHighPrice)
        {
            PriceRange range = new PriceRange{
                lowPrice = inLowPrice,
                highPrice = inHighPrice,
            };

            range.RoundPrices();

            Assert.Equal(range.lowPrice, outLowPrice);
            Assert.Equal(range.highPrice, outHighPrice);
        }
    }
}