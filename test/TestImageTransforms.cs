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

            composedImage.Save(@"testArt/perspectiveTransform0.jpg");
        }

        public static Bitmap TransformImage()
        {
            Bitmap sourceImage = Program.LoadImage(Program.LoadImageType.Test, "sourceImage0.jpg");

            Bitmap copiedSourceImage = sourceImage.Clone();

            ImageTransforms.PerspectiveTransform(
                sourceImage,
                copiedSourceImage,
                new SixLabors.Primitives.Point(10, 10),
                new SixLabors.Primitives.Point(250, 50),
                new SixLabors.Primitives.Point(30, 500),
                new SixLabors.Primitives.Point(400, 300));
            return copiedSourceImage;
        }
    }

    public static class TestImageTransforms
    {
        [Fact]
        public static void TransformImageAcceptanceTest()
        {
            Bitmap actualImage = TestImageTransformUtils.TransformImage();

            Bitmap expectedImage = Program.LoadImage(Program.LoadImageType.Test, "perspectiveTransform0.jpg");

            TestUtils.AssertImagesAreTheSame(expectedImage, actualImage);
        }
    }
}