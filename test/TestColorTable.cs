
using Xunit;

namespace AppraisalBot
{
    public static class TestColorTable
    {
        [Theory]
        [InlineData("000000", "black")]
        [InlineData("FFFFFF", "white")]
        [InlineData("FF0000", "red")]
        [InlineData("00FF00", "green")]
        [InlineData("0000FF", "blue")]
        public static void ColorTableFindsClosestColorNamesFromHexStrings(string colorHexString, string expectedColorName)
        {
            Assert.Equal(expectedColorName, ColorTable.GetClosestColorName(colorHexString));
        }
    }
}