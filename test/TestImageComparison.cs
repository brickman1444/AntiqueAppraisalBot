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
    }
}

