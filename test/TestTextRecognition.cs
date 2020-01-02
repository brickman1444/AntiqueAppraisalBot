using System;
using System.IO;

using SixLabors.ImageSharp;

using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    public static class TestTextRecognition
    {
        public static void Run()
        {
            Bitmap sourceImage = Program.LoadImage(Program.LoadImageType.Test, "sourceArtWithEnglishText.jpg");

            Microsoft.ProjectOxford.Vision.VisionServiceClient visionServiceClient = Program.GetVisionServiceClient();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                sourceImage.SaveAsPng(memoryStream);
                memoryStream.Position = 0;

                Console.WriteLine("Calling VisionServiceClient.AnalyzeImageInDomainAsync()...");

                // English - en into United States, 404 into year, "New" "York" into location?

                Microsoft.ProjectOxford.Vision.Contract.OcrResults result = visionServiceClient.RecognizeTextAsync(memoryStream).GetAwaiter().GetResult();

                string location = LanguageCodeToLocation.LookUp(result.Language);

                Console.WriteLine("Location: " + location);
            }
        }
    }
}