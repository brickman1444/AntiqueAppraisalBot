using System;

using Xunit;
using SixLabors.ImageSharp.Advanced;

using PixelColor = SixLabors.ImageSharp.PixelFormats.Rgba32;
using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    public static class TestUtils
    {
        public static Random GetDeterministicRandom()
        {
            return new Random(0);
        }

        public static void AssertImagesAreTheSame(Bitmap imageA, Bitmap imageB)
        {
            float percentDifference = GetPercentDifference(imageA, imageB);
            Assert.InRange(percentDifference, 0.0f, 0.001f);
        }

        public static float GetPercentDifference(Bitmap imageA, Bitmap imageB, float perPixelThreshold = 0.02f)
        {
            if (imageA.Height != imageB.Height || imageA.Width != imageB.Width)
            {
                return 100.0f;
            }

            float totalPixels = imageA.Height * imageA.Width;
            float amountOfPixelDifference = 0.0f;

            for (int y = 0; y < imageA.Height; y++)
            {
                Span<PixelColor> aSpan = imageA.GetPixelRowSpan(y);
                Span<PixelColor> bSpan = imageB.GetPixelRowSpan(y);

                for (int x = 0; x < imageA.Width; x++)
                {
                    PixelColor aPixel = aSpan[x];
                    PixelColor bPixel = bSpan[x];

                    float pixelDifference = GetPixelDifference(ref aPixel, ref bPixel);

                    if (pixelDifference >= perPixelThreshold)
                    {
                        amountOfPixelDifference += pixelDifference;
                    }
                }
            }

            return amountOfPixelDifference / totalPixels;
        }

        public static float GetPixelDifference(ref PixelColor a, ref PixelColor b)
        {
            float pixelDistance = GetManhattanDistanceInRgbaSpace(ref a, ref b);

            return pixelDistance / (255.0f * 4.0f);
        }

        private static int GetManhattanDistanceInRgbaSpace(ref PixelColor a, ref PixelColor b)
        {
            return Diff(a.R, b.R) + Diff(a.G, b.G) + Diff(a.B, b.B) + Diff(a.A, b.A);
        }

        private static int Diff(ushort a, ushort b) => Math.Abs(a - b);
    }
}