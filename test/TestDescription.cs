
using Xunit;

using Caption = Microsoft.ProjectOxford.Vision.Contract.Caption;

namespace AppraisalBot
{
    public static class TestDescription
    {
        static Caption ABook = new Caption{Text = "A book",};
        static Caption AVase = new Caption{Text = "A vase",};
        static Caption APerson = new Caption{Text = "A person",};
        static Caption AVeryComplicatedBook = new Caption{Text = "A very complicated book",};
        static Caption AVeryComplicatedVase = new Caption{Text = "A very complicated vase",};
        static Caption AnOldPhotoOfAPerson = new Caption{Text = "An old photo of a person",};
        static Caption APersonSittingOnABook = new Caption{Text = "A person sitting on a book",};
        static Caption AGroupOfPeoplePosingForTheCamera = new Caption{Text = "A group of people posing for the camera",};
        static Caption AGroupOfPeoplePosingForAPhotoInFrontOfAWindow = new Caption{Text = "A group of people posing for a photo in front of a window",};
        static Caption AManStandingNextToAVase = new Caption{Text = "A man standing next to a vase"};
        static Caption AGroupOfStuffedAnimalsSittingNextToAWoman = new Caption{Text = "A group of stuffed animals sitting next to a woman"};
        static Caption AWomanStandingInFrontOfAWindow = new Caption{Text = "A woman standing in front of a window"};

        static Description.Arguments Blue = new Description.Arguments{foregroundColor = "blue"};
        static Description.Arguments BlueBlackAndWhite = new Description.Arguments{foregroundColor = "blue", isBlackAndWhite = true};
        static Description.Arguments BlueOldBlackAndWhite = new Description.Arguments{foregroundColor = "blue", isOld = true, isBlackAndWhite = true};
        static Description.Arguments BluePainting = new Description.Arguments{foregroundColor = "blue", isPainting = true};
        
        static Description.Arguments Red = new Description.Arguments{foregroundColor = "red"};
        static Description.Arguments RedOld = new Description.Arguments{foregroundColor = "red", isOld = true};
        static Description.Arguments RedOldBlackAndWhite = new Description.Arguments{foregroundColor = "red", isOld = true, isBlackAndWhite = true};
        static Description.Arguments RedPhoto = new Description.Arguments{foregroundColor = "red", isPhoto = true};
        static Description.Arguments RedPrint = new Description.Arguments{foregroundColor = "red", isSign = true};
        static Description.Arguments RedOldPhoto = new Description.Arguments{foregroundColor = "red", isOld = true, isPhoto = true};

        public static object[][] ComplexItems = new object[][]{
            new object[]{AVeryComplicatedBook, Blue, "A very complicated book" },
            new object[]{AVeryComplicatedBook, BlueBlackAndWhite, "A very complicated book" },
            new object[]{AVeryComplicatedVase, RedOld, "A very complicated vase" },
            new object[]{AVeryComplicatedVase, RedOldBlackAndWhite, "A very complicated vase" },
            new object[]{AnOldPhotoOfAPerson, RedOldPhoto, "An old photo of a person" },
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

        public static object[][] People = new object[][]{
            new object[]{APerson, Blue, "A statue of a person" },
            new object[]{APerson, BluePainting, "A painting of a person" },
            new object[]{APerson, RedPhoto, "A photo of a person" },
            new object[]{APerson, RedPrint, "A print of a person" },
            new object[]{AGroupOfPeoplePosingForTheCamera, RedPhoto, "A photo of a group of people posing for the camera" },
            new object[]{AGroupOfPeoplePosingForAPhotoInFrontOfAWindow, RedPhoto, "A photo of a group of people posing for a photo in front of a window" },
            new object[]{APersonSittingOnABook, BluePainting, "A painting of a person sitting on a book" },
        };

        [Theory]
        [MemberData(nameof(People))]
        public static void WhenCaptionIncludesPeopleThenDescriptionAddsArtQualifier(Caption caption, Description.Arguments arguments, string expectedDescription)
        {
            string actualDescription = Description.Get(caption, arguments);
            Assert.Equal(expectedDescription, actualDescription);
        }

        public static object[][] MenAndWomen = new object[][]{
            new object[]{AManStandingNextToAVase, Blue, "A statue of a person standing next to a vase" },
            new object[]{AGroupOfStuffedAnimalsSittingNextToAWoman, BluePainting, "A group of stuffed animals sitting next to a person" },
            new object[]{AWomanStandingInFrontOfAWindow, RedPhoto, "A photo of a person standing in front of a window" },
        };

        [Theory]
        [MemberData(nameof(MenAndWomen))]
        public static void DescriptionsAreGenderNeutral(Caption caption, Description.Arguments arguments, string expectedDescription)
        {
            string actualDescription = Description.Get(caption, arguments);
            Assert.Equal(expectedDescription, actualDescription);
        }
    }
}
