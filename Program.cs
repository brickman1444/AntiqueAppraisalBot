using System;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;

using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

using System.Drawing;
using System.Drawing.Imaging;

namespace AppraisalBot
{
    class MetResponse
    {
        public Object request;
        public List<MetResult> results;
        public List<Object> facets;
        public int totalResults;
        public int totalCollectionResults;
        public bool fromCache;
        public bool isCorrected;
        public string originalQuery;
        public string correctedQuery;
    }

    class MetResult
    {
        public string title;
        public string description;
        public string teaserText;
        public string url;
        public string image;
        public string regularImage;
        public string largeImage;
        public string date;
        public string medium;
        public string accessionNumber;
        public string galleryInformation;
    }

    struct PriceRange
    {
        public int lowPrice;
        public int highPrice;
    }

    class Program
    {
        static string computerVisionKey = "";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Delete the previous output
            string[] filePaths = Directory.GetFiles(@"images\");
            foreach (string filePath in filePaths)
            {
                File.Delete(filePath);
            }

            using ( StreamReader fs = new StreamReader( "localconfig/computervisionkey.txt" ) )
            {
                computerVisionKey = fs.ReadToEnd();
            }

            int numItems = 3;

            Random rnd = new Random();
            int collectionOffset = rnd.Next(0,1950);

            string responseText = GetCollectionListing( numItems, collectionOffset );
            MetResponse responseObject = JsonConvert.DeserializeObject<MetResponse>(responseText);

            Console.WriteLine( "Found " + responseObject.results.Count + " results" );

            for ( int i = 0; i < responseObject.results.Count; i++ )
            {
                Console.WriteLine("small url: " + responseObject.results[i].image);
                Console.WriteLine("large url: " + responseObject.results[i].largeImage );

                string smallImageUrl = responseObject.results[i].image;
                int index = smallImageUrl.IndexOf( responseObject.results[i].largeImage.Substring(0,3) );
                string largeImageUrl = smallImageUrl.Substring(0,index) + responseObject.results[i].largeImage;

                Bitmap image = DownloadImage( largeImageUrl );
                bool doAnalysis = true;
                if (image != null && doAnalysis)
                {
                    AnalysisResult analysisResult = AnalyzeImage( image ).GetAwaiter().GetResult();

                    CreateAppraisal( image, @"images/image" + i + ".jpg", analysisResult );
                }
                
            }

            Console.WriteLine("Done");
        }

        static string GetCollectionListing(int numItems, int offset)
        {
            string[] materials = {
                "Bags",
                "Jewelry",
                "Sculpture",
                "Bowls",
                "Furniture",
                "Musical Instruments",
                "Vessels",
                "Ceramics",
                "Wood",
                "Paintings",
            };

            Random rnd = new Random();
            string material = materials[ rnd.Next(0, materials.Length)];

            Console.WriteLine("Material: " + material + " offset: " + offset + " numItems: " + numItems);

            string url = "http://metmuseum.org/api/collection/collectionlisting?offset=" + offset + "&pageSize=0&perPage=" + numItems + "&sortBy=Relevance&sortOrder=asc&material=" + material;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();

            Stream receiveStream = response.GetResponseStream();
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            // Pipes the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader(receiveStream, encode);
            Console.WriteLine("\r\nResponse stream received.");

            string responseText = readStream.ReadToEnd();

            // Releases the resources of the response.
            response.Close();
            // Releases the resources of the Stream.
            readStream.Close();

            return responseText;
        }

        static Bitmap DownloadImage(string url)
        {
            try 
            {
                HttpWebRequest lxRequest = (HttpWebRequest)WebRequest.Create(url);

                // returned values are returned as a stream, then read into a string
                using (HttpWebResponse lxResponse = (HttpWebResponse)lxRequest.GetResponse()){
                    
                    Bitmap image = new Bitmap( lxResponse.GetResponseStream() );

                    if (image.Width >= 250)
                    {
                        return image;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("exception thrown during get for " + url);
            }
            return null;
        }

        static async Task<AnalysisResult> AnalyzeImage(Bitmap sourceImage)
        {
                VisionServiceClient VisionServiceClient = new VisionServiceClient(computerVisionKey, "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");
            Console.WriteLine("VisionServiceClient is created");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                sourceImage.Save( memoryStream, sourceImage.RawFormat );
                memoryStream.Position = 0;

                //
                // Analyze the image for all visual features
                //
                Console.WriteLine("Calling VisionServiceClient.AnalyzeImageAsync()...");
            VisualFeature[] visualFeatures = new VisualFeature[] { 
                VisualFeature.Adult,
                VisualFeature.Categories,
                VisualFeature.Color,
                VisualFeature.Description
                };
                AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync( memoryStream, visualFeatures);
                return analysisResult;
            }
        }

        static PriceRange GetPriceRange(string caption, Bitmap image, double confidence)
        {
            PriceRange priceRange;
            priceRange.lowPrice = 0;
            priceRange.highPrice = 0;

            foreach ( char c in caption )
            {
                priceRange.highPrice += c;
            }

            priceRange.highPrice += image.Width + image.Height;

            System.Drawing.Color pixelSampleColor = image.GetPixel( image.Width / 2, image.Height / 2 );

            float red = (float)pixelSampleColor.R / 255.0f;
            float green = (float)pixelSampleColor.G / 255.0f;
            float blue = (float)pixelSampleColor.B / 255.0f;
            priceRange.highPrice *= (int)( 1.0f + red + green + blue );

            priceRange.lowPrice = (int)(priceRange.highPrice * confidence);

            return priceRange;
        }

        static int GetYear( Bitmap image )
        {
            int maxYear = 2017;
            int minYear = 10;

            System.Drawing.Color pixelSampleColor = image.GetPixel( image.Width / 3, image.Height / 3 );

            float red = (float)pixelSampleColor.R / 255.0f;
            float green = (float)pixelSampleColor.G / 255.0f;
            float blue = (float)pixelSampleColor.B / 255.0f;
            
            float scale = (red + green + blue) / 3.0f;

            int year = (int)(minYear + (maxYear - minYear) * scale);
            return year;
        }

        static void CreateAppraisal( Bitmap sourceImage, string destinationFilePath, AnalysisResult analysisResult )
        {
            Caption caption = GetCaption( analysisResult );
            Console.WriteLine( "Caption: " + caption.Text + " " + caption.Confidence );

            string descriptionText = GetDescription( caption );
            float confidence = (float)caption.Confidence;

            Bitmap composedImage = ComposeImage( sourceImage, descriptionText, confidence );

            composedImage.Save( destinationFilePath );
        }

        static Caption GetCaption( AnalysisResult analysisResult )
        {
            Caption caption = new Caption();
            caption.Text = "Something";
            caption.Confidence = 0.0001f;

            foreach ( Caption c in analysisResult.Description.Captions )
            {
                if ( c.Confidence > caption.Confidence )
                {
                    caption = c;
                }
            }

            return caption;
        }

        static string GetDescription( Caption caption )
        {
            // Filter and adjust the caption
            string descriptionText = caption.Text;
            descriptionText = descriptionText.Replace("a close up of ", "");
            descriptionText = descriptionText.Replace(" sitting on a table", "");
            descriptionText = descriptionText.Replace(" on a table", "");

            // Capitalize the first letter
            descriptionText = char.ToUpper(descriptionText[0]) + descriptionText.Substring(1);

            return descriptionText;
        }

        static Bitmap ComposeImage(Bitmap sourceImage, string descriptionText, float confidence)
        {
            Bitmap loadedBitmap = sourceImage;

            // There's some exception that's thrown when creating a Graphics from an "indexed bitmap"
            // which some of the images are. You have to create a new bitmap and that works.
            Bitmap drawnBitmap = new Bitmap( loadedBitmap );
            Graphics graphics = Graphics.FromImage(drawnBitmap);

            PriceRange priceRange = GetPriceRange( descriptionText, drawnBitmap, confidence );
            int year = GetYear( drawnBitmap );

            string fullCaption = descriptionText + " (ca. " + year + ")\n $" + priceRange.lowPrice + "-$" + priceRange.highPrice;

            Bitmap footerImage = (Bitmap)Image.FromFile(@"sourceArt/footer.png");

            float scale = (float)drawnBitmap.Width / (float)footerImage.Width;
            float footerHeight = scale * footerImage.Height;
            float footerOriginY = drawnBitmap.Height - footerHeight;

            graphics.DrawImage( footerImage, 0, footerOriginY, drawnBitmap.Width, footerHeight );

            float textOriginY = footerOriginY + 25.0f * scale;
            float textOriginX = 200.0f * scale;

            int fontSize = (int)(25 * scale);

            Font drawFont = new Font("Arial", fontSize, FontStyle.Bold);
            SolidBrush grayBrush = new SolidBrush(System.Drawing.Color.MidnightBlue);
            graphics.DrawString(fullCaption, drawFont, grayBrush, textOriginX + 1, textOriginY + 1);
            SolidBrush whiteBrush = new SolidBrush(System.Drawing.Color.White);
            graphics.DrawString(fullCaption, drawFont, whiteBrush, textOriginX, textOriginY);

            return drawnBitmap;
        }
    }
}
