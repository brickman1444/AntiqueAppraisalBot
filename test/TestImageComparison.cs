using SixLabors.ImageSharp;
using Xunit;

using PixelColor = SixLabors.ImageSharp.PixelFormats.Rgba32;
using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    public static class TestImageComparison
    {
        [Fact]
        public static void SinglePixelComparison()
        {
            PixelColor white = Color.White;
            PixelColor transparent = Color.Transparent;
            Assert.Equal(1.0f, TestUtils.GetPixelDifference(ref white, ref transparent));
            Assert.Equal(0.0f, TestUtils.GetPixelDifference(ref white, ref white));
        }

        [Fact]
        public static void WhiteAndTransparentSquaresAreTotallyDifferent()
        {
            Bitmap white = Program.LoadImage(Program.LoadImageType.Test, "10x10WhiteSquare.png");
            Bitmap transparent = Program.LoadImage(Program.LoadImageType.Test, "10x10TransparentBlackSquare.png");

            Assert.Equal(1.0f, TestUtils.GetPercentDifference(white, transparent));
        }

        [Fact]
        public static void WhiteAndBlackSquaresAreMostlyDifferent()
        {
            Bitmap white = Program.LoadImage(Program.LoadImageType.Test, "10x10WhiteSquare.png");
            Bitmap black = Program.LoadImage(Program.LoadImageType.Test, "10x10BlackSquare.png");

            Assert.Equal(0.75f, TestUtils.GetPercentDifference(white, black));
        }

        [Fact]
        public static void ImageIsIdenticalToIteself()
        {
            Bitmap art = Program.LoadImage(Program.LoadImageType.Test, "paintingWithFace.jpg");

            Assert.Equal(0.0f, TestUtils.GetPercentDifference(art, art));
        }

        [Fact]
        public static void SmallTextDifferencesAreDetected()
        {
            System.Func<string, Bitmap> MakeAppraisalImage = (string description) => {
                return Program.ComposeImage(
                    new Bitmap(425, 625), // Relatively accurate background size for result images,
                    description,
                    0.5f,
                    false,
                    false,
                    1.0f,
                    false,
                    false,
                    9876,
                    "Neverland",
                    TestUtils.GetDeterministicRandom());
            };

            Bitmap ramp = MakeAppraisalImage("ramp");
            Program.SaveTestImage(ramp, "actual/ramp.jpg");

            Bitmap romp = MakeAppraisalImage("romp");
            Program.SaveTestImage(romp, "actual/romp.jpg");

            TestUtils.AssertImagesAreDifferent(ramp, romp);
        }
    }
}

