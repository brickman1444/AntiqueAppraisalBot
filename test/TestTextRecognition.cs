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

            // English - en into United States, 404 into year, "New" "York" into location?

            Microsoft.ProjectOxford.Vision.Contract.OcrResults result = ComputerVisionService.AnalyzeText(sourceImage);

            string location = LanguageCodeToLocation.LookUp(result.Language);

            Console.WriteLine("Location: " + location);
        }
    }
}