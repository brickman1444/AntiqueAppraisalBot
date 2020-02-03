using System;

using Xunit;

using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    public static class TestProgram
    {
        [Fact]
        public static void TestIsRunningTests()
        {
            Assert.True(Program.IsRunningTests());
        }

        [Fact]
        public static void TestDeterministicRandomFromImage()
        {
            Bitmap image1 = Program.LoadImage(Program.LoadImageType.Test, "sourceImage0.jpg");
            Bitmap image2 = Program.LoadImage(Program.LoadImageType.Test, "sourceImage0.jpg");

            Random random1 = Program.GetDeterministicRandom(image1);
            Random random2 = Program.GetDeterministicRandom(image2);

            Assert.Equal(random1.Next(), random2.Next());
            Assert.Equal(random1.Next(), random2.Next());
            Assert.Equal(random1.Next(), random2.Next());
        }
    }
}