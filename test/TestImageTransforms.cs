using SixLabors.ImageSharp;
using Xunit;

using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    public static class TestImageTransformUtils
    {
        public static void UpdateExpectedOutput()
        {
            Bitmap perspectiveTransformExpected = TransformImage();
            perspectiveTransformExpected.Save(@"testArt/perspectiveTransformExpected.jpg");

            Bitmap composedExpected = ComposeOntoBackground();
            composedExpected.Save(@"testArt/composedImageExpected.jpg");
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

        public static Bitmap ComposeOntoBackground()
        {
            Bitmap sourceImage = Program.LoadImage(Program.LoadImageType.Test, "perspectiveTransformSource.jpg");

            return ImageTransforms.ComposeImageOntoPhoto(sourceImage);
        }
    }

    public static class TestImageTransforms
    {
        [Fact]
        public static void TransformImageAcceptanceTest()
        {
            Bitmap actualImage = TestImageTransformUtils.TransformImage();
            Program.SaveTestImage(actualImage, "perspectiveTransformActual.jpg");

            Bitmap expectedImage = Program.LoadImage(Program.LoadImageType.Test, "perspectiveTransformExpected.jpg");

            TestUtils.AssertImagesAreTheSame(expectedImage, actualImage);
        }

        [Fact]
        public static void ComposeOntoBackgroundAcceptanceTest()
        {
            Bitmap actualImage = TestImageTransformUtils.ComposeOntoBackground();

            Program.SaveTestImage(actualImage, "composedImageActual.jpg");

            Bitmap expectedImage = Program.LoadImage(Program.LoadImageType.Test, "composedImageExpected.jpg");

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