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
    }
}

