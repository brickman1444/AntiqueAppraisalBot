using System;
using System.IO;
using System.Reflection;

using SixLabors.ImageSharp;
using Xunit;

using PixelColor = SixLabors.ImageSharp.PixelFormats.Rgba32;
using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    public static class TestComposeImage
    {
        [Fact]
        public static void ComposeImageAcceptanceTest()
        {
            var assembly = Assembly.GetAssembly(typeof(Program));
            var resourceStream = assembly.GetManifestResourceStream("AppraisalBot.testArt.sourceImage0.jpg");

            Assert.NotNull(resourceStream);

            string[] resourceNames = assembly.GetManifestResourceNames();

            //Assert.Equal("", assembly.GetName().Name);

            Bitmap image = Image.Load<PixelColor>(resourceStream);

            Assert.Equal(0, image.Height);

            //using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            //{
            //    return await reader.ReadToEndAsync();
            //}

            //string path = Path.GetRelativePath(Directory.GetCurrentDirectory(), @"testArt/sourceImage0.jpg");

            //Console.WriteLine(Directory.GetCurrentDirectory());
            //Console.WriteLine(path);

            //Assert.True(File.Exists(path), Assembly.GetExecutingAssembly().GetName().CodeBase);

            //Bitmap sourceImage = Image.Load<PixelColor>(@"testArt/sourceImage0.jpg");
        }
    }
}