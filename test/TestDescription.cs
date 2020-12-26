
using Xunit;

using Caption = Microsoft.ProjectOxford.Vision.Contract.Caption;

namespace AppraisalBot
{
    public static class TestDescription
    {
        static Caption ABook = new Caption{Text = "A book",};
        static Caption AVase = new Caption{Text = "A vase",};
        static Caption AVeryComplicatedBook = new Caption{Text = "A very complicated book",};
        static Caption AVeryComplicatedVase = new Caption{Text = "A very complicated vase",};

        static Description.Arguments Blue = new Description.Arguments{foregroundColor = "blue"};
        static Description.Arguments BlueBlackAndWhite = new Description.Arguments{foregroundColor = "blue", isBlackAndWhite = true};
        static Description.Arguments BlueOldBlackAndWhite = new Description.Arguments{foregroundColor = "blue", isOld = true, isBlackAndWhite = true};
        static Description.Arguments Red = new Description.Arguments{foregroundColor = "red"};
        static Description.Arguments RedOld = new Description.Arguments{foregroundColor = "red", isOld = true};
        static Description.Arguments RedOldBlackAndWhite = new Description.Arguments{foregroundColor = "red", isOld = true, isBlackAndWhite = true};

        public static object[][] ComplexItems = new object[][]{
            new object[]{AVeryComplicatedBook, Blue, "A very complicated book" },
            new object[]{AVeryComplicatedBook, BlueBlackAndWhite, "A very complicated book" },
            new object[]{AVeryComplicatedVase, RedOld, "A very complicated vase" },
            new object[]{AVeryComplicatedVase, RedOldBlackAndWhite, "A very complicated vase" },
        };

        [Theory]
        [MemberData(nameof(ComplexItems))]
        public static void WhenCaptionsAreComplexThenDescriptionDoesntChange(Caption caption, Description.Arguments arguments, string expectedDescription)
        {
            string actualDescription = Description.Get(caption, arguments);
            Assert.Equal(expectedDescription, actualDescription);
        }

        public static object[][] SimpleItems = new object[][]{
            new object[]{ABook, Blue, "A blue book" },
            new object[]{AVase, Red, "A red vase" },
        };

        [Theory]
        [MemberData(nameof(SimpleItems))]
        public static void WhenCaptionsAreSimpleThenDescriptionHasColor(Caption caption, Description.Arguments arguments, string expectedDescription)
        {
            string actualDescription = Description.Get(caption, arguments);
            Assert.Equal(expectedDescription, actualDescription);
        }

        public static object[][] SimpleOldBlackAndWhiteItems = new object[][]{
            new object[]{ABook, BlueOldBlackAndWhite, "An old book" },
            new object[]{AVase, RedOldBlackAndWhite, "An old vase" },
        };

        [Theory]
        [MemberData(nameof(SimpleOldBlackAndWhiteItems))]
        public static void WhenCaptionsAreSimpleAndItemIsOldAndBlackAndWhiteThenDescriptionHasOld(Caption caption, Description.Arguments arguments, string expectedDescription)
        {
            string actualDescription = Description.Get(caption, arguments);
            Assert.Equal(expectedDescription, actualDescription);
        }
    }
}
