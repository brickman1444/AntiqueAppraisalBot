using SixLabors.ImageSharp;
using Xunit;

using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    public static class TestImageTransforms
    {
        [Fact]
        public static void TransformImageAcceptanceTest()
        {
            Bitmap sourceImage = Program.LoadImage(Program.LoadImageType.Test, "perspectiveTransformSource.jpg");

            Bitmap actualImage = sourceImage.Clone();

            ImageTransforms.PerspectiveTransform(
                sourceImage,
                actualImage,
                new SixLabors.ImageSharp.Point(10, 10),
                new SixLabors.ImageSharp.Point(250, 50),
                new SixLabors.ImageSharp.Point(30, 500),
                new SixLabors.ImageSharp.Point(400, 300));

            Program.SaveTestImage(actualImage, "actual/perspectiveTransform.jpg");

            Bitmap expectedImage = Program.LoadImage(Program.LoadImageType.Test, "expected/perspectiveTransform.jpg");

            TestUtils.AssertImagesAreTheSame(expectedImage, actualImage);
        }

        [Fact]
        public static void ComposeOntoBackgroundAcceptanceTest()
        {
            Bitmap sourceImage = Program.LoadImage(Program.LoadImageType.Test, "perspectiveTransformSource.jpg");

            Bitmap actualImage = ImageTransforms.ComposeImageOntoPhoto(sourceImage);

            Program.SaveTestImage(actualImage, "actual/composedImage.jpg");

            Bitmap expectedImage = Program.LoadImage(Program.LoadImageType.Test, "expected/composedImage.jpg");

            TestUtils.AssertImagesAreTheSame(expectedImage, actualImage);
        }

        [Theory]
        [InlineData("veryTallImage.jpg")]
        [InlineData("veryWideImage.jpg")]
        public static void ResizeImageToWithinAnalysisLimitsTest(string filename)
        {
            Bitmap sourceImage = Program.LoadImage(Program.LoadImageType.Test, filename);

            Assert.False(ImageTransforms.IsWithinAnalysisLimits(sourceImage));

            Bitmap resizedImage = ImageTransforms.ResizeToWithinAnalysisLimits(sourceImage);

            Assert.True(ImageTransforms.IsWithinAnalysisLimits(resizedImage));
        }
    }
}