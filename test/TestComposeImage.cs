using SixLabors.ImageSharp;
using Xunit;

using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    public static class TestComposeImage
    {
        [Fact]
        public static void ComposeImageAcceptanceTest()
        {
            Bitmap sourceImage = Program.LoadImage(Program.LoadImageType.Test, "paintingWithFace.jpg");

            Bitmap actualImage = Program.ComposeImage(
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

            Bitmap expectedImage = Program.LoadImage(Program.LoadImageType.Test, "expected/composedWithCaption.jpg");

            TestUtils.AssertImagesAreTheSame(expectedImage, actualImage);
        }
    }
}

