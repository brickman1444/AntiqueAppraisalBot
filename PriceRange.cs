using System;

namespace AppraisalBot
{
    struct PriceRange
    {
        public int lowPrice;
        public int highPrice;

        public static PriceRange RoundPrices(PriceRange inputRange)
        {
            PriceRange roundedRange = inputRange;
            
            // Get the order of magnitude of the higher price.
            int highPriceOrder = (int)Math.Floor(Math.Log10(roundedRange.highPrice));

            highPriceOrder -= 1; // subtract 1 to keep two meaningful digits on at least the high price

            if (highPriceOrder <= 0)
            {
                // If the prices are two small, exit early
                return roundedRange;
            }

            int highPriceScale = (int)Math.Pow(10, highPriceOrder);

            // Use integer division to truncate off the end
            roundedRange.highPrice = (roundedRange.highPrice / highPriceScale) * highPriceScale;

            if (roundedRange.lowPrice / highPriceScale > 0)
            {
                // If the prices are generally close we can use the high price's
                // scale to round the low price. This is usually nice to read
                roundedRange.lowPrice = (roundedRange.lowPrice / highPriceScale) * highPriceScale;
            }
            else
            {
                // If the prices are too different, round the low price on its own
                int lowPriceOrder = (int)Math.Floor(Math.Log10(roundedRange.lowPrice));

                // Don't subtract anything from the low price order. The prices
                // are different enough that we only want one meaningful digit. 

                if (lowPriceOrder > 0)
                {
                    int lowPriceScale = (int)Math.Pow(10, lowPriceOrder);
                    roundedRange.lowPrice = (roundedRange.lowPrice / lowPriceScale) * lowPriceScale;
                }
            }

            return roundedRange;
        }
    }
}