
using Xunit;

using Caption = Microsoft.ProjectOxford.Vision.Contract.Caption;

namespace AppraisalBot
{
    public static class TestCDescription
    {
        static Caption ABook = new Caption{Text = "A book",};
        static Caption AVase = new Caption{Text = "A vase",};
        static Caption AVeryComplicatedBook = new Caption{Text = "A very complicated book",};
        static Caption AVeryComplicatedVase = new Caption{Text = "A very complicated vase",};

        public static object[][] ComplexItems = new object[][]{
            new object[]{AVeryComplicatedBook, "blue", false, false, "A very complicated book" },
            new object[]{AVeryComplicatedBook, "blue", false, true, "A very complicated book" },
            new object[]{AVeryComplicatedVase, "red", true, false, "A very complicated vase" },
            new object[]{AVeryComplicatedVase, "red", true, true, "A very complicated vase" },
        };

        [Theory]
        [MemberData(nameof(ComplexItems))]
        public static void WhenCaptionsAreComplexThenDescriptionDoesntChange(Caption caption, string foregroundColor, bool isOld, bool isBlackAndWhite, string expectedDescription)
        {
            string actualDescription = Description.Get(caption, foregroundColor, isOld, isBlackAndWhite);
            Assert.Equal(expectedDescription, actualDescription);
        }

        public static object[][] SimpleItems = new object[][]{
            new object[]{ABook, "blue", false, false, "A blue book" },
            new object[]{AVase, "red", false, false, "A red vase" },
        };

        [Theory]
        [MemberData(nameof(SimpleItems))]
        public static void WhenCaptionsAreSimpleThenDescriptionHasColor(Caption caption, string foregroundColor, bool isOld, bool isBlackAndWhite, string expectedDescription)
        {
            string actualDescription = Description.Get(caption, foregroundColor, isOld, isBlackAndWhite);
            Assert.Equal(expectedDescription, actualDescription);
        }

        public static object[][] SimpleOldBlackAndWhiteItems = new object[][]{
            new object[]{ABook, "blue", true, true, "An old book" },
            new object[]{AVase, "red", true, true, "An old vase" },
        };

        [Theory]
        [MemberData(nameof(SimpleOldBlackAndWhiteItems))]
        public static void WhenCaptionsAreSimpleAndItemIsOldAndBlackAndWhiteThenDescriptionHasOld(Caption caption, string foregroundColor, bool isOld, bool isBlackAndWhite, string expectedDescription)
        {
            string actualDescription = Description.Get(caption, foregroundColor, isOld, isBlackAndWhite);
            Assert.Equal(expectedDescription, actualDescription);
        }
    }
}
