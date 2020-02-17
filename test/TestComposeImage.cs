using SixLabors.ImageSharp;
using Xunit;

using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    public static class TestComposeImageUtils
    {
        public static void UpdateExpectedOutput()
        {
            Bitmap composedImage = ComposeImage();

            composedImage.Save(@"testArt/finalImage0.jpg");
        }

        public static Bitmap ComposeImage()
        {
            Bitmap sourceImage = Program.LoadImage(Program.LoadImageType.Test, "sourceImage0.jpg");

            return Program.ComposeImage(
                sourceImage,
                "test description that is kind of long and has multiple lines",
                0.5f,
                false,
                false,
                1.0f,
                false,
                false,
                9876,
                "Neverland",
                TestUtils.GetDeterministicRandom());
        }
    }

    public static class TestComposeImage
    {
        [Fact]
        public static void ComposeImageAcceptanceTest()
        {
            Bitmap actualImage = TestComposeImageUtils.ComposeImage();

            Bitmap expectedImage = Program.LoadImage(Program.LoadImageType.Test, "finalImage0.jpg");

            TestUtils.AssertImagesAreTheSame(expectedImage, actualImage);
        }
    }
}

