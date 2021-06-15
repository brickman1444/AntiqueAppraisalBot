using SixLabors.ImageSharp;
using Xunit;

using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    public static class TestImageTransformUtils
    {
        public static void UpdateExpectedOutput()
        {
            Bitmap composedImage = TransformImage();

            composedImage.Save(@"testArt/perspectiveTransformExpected.jpg");
        }

        public static Bitmap TransformImage()
        {
            Bitmap sourceImage = Program.LoadImage(Program.LoadImageType.Test, "perspectiveTransformSource.jpg");

            Bitmap copiedSourceImage = sourceImage.Clone();

            ImageTransforms.PerspectiveTransform(
                sourceImage,
                copiedSourceImage,
                new SixLabors.ImageSharp.Point(10, 10),
                new SixLabors.ImageSharp.Point(250, 50),
                new SixLabors.ImageSharp.Point(30, 500),
                new SixLabors.ImageSharp.Point(400, 300));
            return copiedSourceImage;
        }
    }

    public static class TestImageTransforms
    {
        [Fact]
        public static void TransformImageAcceptanceTest()
        {
            Bitmap actualImage = TestImageTransformUtils.TransformImage();

            Bitmap expectedImage = Program.LoadImage(Program.LoadImageType.Test, "perspectiveTransformExpected.jpg");

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