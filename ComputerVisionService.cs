using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using SixLabors.ImageSharp;

using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    public static class ComputerVisionService
    {
        public class CelebrityAnalysisResult
        {
            public class Celebrity
            {
                public class FaceRectangle
                {
                    public int top;
                    public int left;
                    public int width;
                    public int height;
                }

                public FaceRectangle faceRectangle;
                public string name;
                public float confidence;
            }

            public List<Celebrity> celebrities;
        }

        public class AnalysisBlob
        {
            public AnalysisResult generalAnalysisResult;
            public OcrResults ocrAnalysisResult;
        }

        private static string computerVisionKey = null;
        private static VisionServiceClient GetClient()
        {
            if (computerVisionKey == null)
            {
                computerVisionKey = System.Environment.GetEnvironmentVariable("computerVisionKey");

                if (computerVisionKey == null)
                {
                    using (StreamReader fs = File.OpenText("localconfig/computervisionkey.txt"))
                    {
                        computerVisionKey = fs.ReadToEnd();
                    }
                }
            }

            return new VisionServiceClient(computerVisionKey, "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");
        }

        public static AnalysisBlob GetAnalysisBlob(Bitmap image)
        {
            AnalysisBlob analysisBlob = new AnalysisBlob();
            analysisBlob.generalAnalysisResult = ComputerVisionService.AnalyzeImage(image);
            analysisBlob.ocrAnalysisResult = ComputerVisionService.AnalyzeText(image);
            return analysisBlob;
        }

        public static CelebrityAnalysisResult AnalyzeImageForCelebrities(Bitmap sourceImage)
        {
            VisionServiceClient VisionServiceClient = ComputerVisionService.GetClient();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                sourceImage.SaveAsPng(memoryStream);
                memoryStream.Position = 0;

                Console.WriteLine("Calling VisionServiceClient.AnalyzeImageInDomainAsync()...");

                // This is how you'd recognize celebrities like Henry Clay
                Microsoft.ProjectOxford.Vision.Contract.AnalysisInDomainResult result = VisionServiceClient.AnalyzeImageInDomainAsync(memoryStream, "celebrities").GetAwaiter().GetResult();

                Newtonsoft.Json.Linq.JObject jsonObj = result.Result as Newtonsoft.Json.Linq.JObject;

                CelebrityAnalysisResult celebResult = jsonObj.ToObject<CelebrityAnalysisResult>() as CelebrityAnalysisResult;
                return celebResult;
            }
        }

        public static OcrResults AnalyzeText(Bitmap sourceImage)
        {
            Microsoft.ProjectOxford.Vision.VisionServiceClient visionServiceClient = ComputerVisionService.GetClient();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                sourceImage.SaveAsPng(memoryStream);
                memoryStream.Position = 0;

                Console.WriteLine("Calling VisionServiceClient.RecognizeTextAsyncs()...");

                return visionServiceClient.RecognizeTextAsync(memoryStream).GetAwaiter().GetResult();
            }
        }

        public static AnalysisResult AnalyzeImage(Bitmap sourceImage)
        {
            VisionServiceClient VisionServiceClient = ComputerVisionService.GetClient();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                sourceImage.SaveAsPng(memoryStream);
                memoryStream.Position = 0;

                Console.WriteLine("Calling VisionServiceClient.AnalyzeImageAsync()...");
                VisualFeature[] visualFeatures = new VisualFeature[] {
                    VisualFeature.Adult,
                    VisualFeature.Color,
                    VisualFeature.Description,
                    VisualFeature.ImageType
                };

                try
                {
                    AnalysisResult analysisResult = VisionServiceClient.AnalyzeImageAsync(memoryStream, visualFeatures).GetAwaiter().GetResult();
                    return analysisResult;
                }
                catch (Microsoft.ProjectOxford.Vision.ClientException exception)
                {
                    Console.WriteLine(exception.Error.Message);
                    Console.WriteLine(exception.Error.Code);
                    
                    throw exception;
                }
            }
        }

        static Bitmap SmartCropImage(Bitmap sourceImage)
        {
            VisionServiceClient VisionServiceClient = ComputerVisionService.GetClient();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                sourceImage.SaveAsPng(memoryStream);
                memoryStream.Position = 0;

                int width = sourceImage.Width;
                int height = (int)(sourceImage.Width / 16.0f * 9.0f * 1.2f); // Set it to a 16:9 with an extra 20% to increase the overall size
                Console.WriteLine("Original Width: " + sourceImage.Width + " Original Height: " + sourceImage.Height + " Cropped Height: " + height);

                if (sourceImage.Height > height)
                {
                    Console.WriteLine("Calling VisionServiceClient.GetThumbnailAsync()...");
                    byte[] bytes = VisionServiceClient.GetThumbnailAsync(memoryStream, width, height).GetAwaiter().GetResult();

                    Bitmap croppedImage = Image.Load(bytes);

                    return croppedImage;
                }
                else
                {
                    Console.WriteLine("Image was already small. No reason to crop");
                    return sourceImage;
                }
            }
        }
    }
}