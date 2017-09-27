using System;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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

    class Appraisal
    {
        public Bitmap image;
        public string comment;

        public Appraisal( Bitmap inImage, string inComment)
        {
            image = inImage;
            comment = inComment;
        }
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

            int numItems = 2;

            MetResponse responseObject = GetCollectionListing( numItems );

            Console.WriteLine( "Found " + responseObject.results.Count + " results" );

            for ( int i = 0; i < responseObject.results.Count; i++ )
            {
                Console.WriteLine("-----------------------------------------------------------------------");

                Console.WriteLine("small url: " + responseObject.results[i].image);
                //Console.WriteLine("large url: " + responseObject.results[i].largeImage );

                string smallImageUrl = responseObject.results[i].image;
                int index = smallImageUrl.LastIndexOf( responseObject.results[i].largeImage.Substring(0,3) );
                string largeImageUrl = smallImageUrl.Substring(0,index) + responseObject.results[i].largeImage;

                Bitmap image = DownloadImage( largeImageUrl );
                bool doAnalysis = true;
                if (image != null && doAnalysis)
                {
                    AnalysisResult analysisResult = AnalyzeImage( image ).GetAwaiter().GetResult();

                    string tagString = "";
                    foreach ( string tag in analysisResult.Description.Tags )
                    {
                        tagString += tag + ", ";
                    }
                    Console.WriteLine( tagString );

                    string accentColor = ColorTable.GetClosestColorName( ColorTable.GetColorFromHexString( analysisResult.Color.AccentColor ) );

                    Console.WriteLine("Foreground: " + analysisResult.Color.DominantColorForeground + " Background: " + analysisResult.Color.DominantColorBackground + " Accent: " + accentColor );

                    Appraisal appraisal = CreateAppraisal( image, analysisResult );

                    string destinationFilePath = @"images/image" + i + ".jpg";
                    appraisal.image.Save( destinationFilePath );
                }
            }

            Console.WriteLine("Done");
        }

        static string GetMetAPIUrl( int offset, int numItems, string material)
        {
            return "http://metmuseum.org/api/collection/collectionlisting?offset=" + offset + "&pageSize=0&perPage=" + numItems + "&sortBy=Relevance&sortOrder=asc&material=" + material + "&showOnly=openaccess";
        }

        static MetResponse GetCollectionListing(int numItems)
        {
            string[] materials = {
                "Bags",
                "Jewelry",
                "Sculpture",
                "Bowls",
                "Furniture",
                "Musical%20instruments",
                "Vessels",
                "Ceramics",
                "Wood",
                "Paintings",
                "Timepieces",
                "Arms",
                "Costume",
                "Flatware",
            };

            Random rnd = new Random();
            string material = materials[ rnd.Next(0, materials.Length)];

            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

            // First get the size of the results so we can randomly pick an offset within that.
            int numItemsInCategory = 0;

            string categorySizeUrl = GetMetAPIUrl( 0, 1, material );

            HttpWebRequest categorySizeRequest = (HttpWebRequest)WebRequest.Create(categorySizeUrl);

            using ( HttpWebResponse categorySizeResponse = (HttpWebResponse)categorySizeRequest.GetResponse() )
            {
                using ( StreamReader readStream = new StreamReader( categorySizeResponse.GetResponseStream(), encode) )
                {
                    string categoryResponseText = readStream.ReadToEnd();

                    MetResponse categoryResponseObject = JsonConvert.DeserializeObject<MetResponse>(categoryResponseText);
                    numItemsInCategory = categoryResponseObject.totalResults;
                }
            }

            Console.WriteLine("Total items in category: " + numItemsInCategory);

            int offset = rnd.Next(0, numItemsInCategory - numItems);

            Console.WriteLine("Material: " + material + " offset: " + offset + " numItems: " + numItems);

            string url = GetMetAPIUrl( offset, numItems, material);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);

            string responseText = "";
            using ( HttpWebResponse response = (HttpWebResponse)myReq.GetResponse() )
            {
                using ( Stream receiveStream = response.GetResponseStream() )
                {
                    // Pipes the stream to a higher level stream reader with the required encoding format. 
                    StreamReader readStream = new StreamReader(receiveStream, encode);
                    responseText = readStream.ReadToEnd();;
                }

            }

            MetResponse responseObject = JsonConvert.DeserializeObject<MetResponse>(responseText);
            return responseObject;
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
                    else
                    {
                        Console.WriteLine("throwing out image because it's too small. Width: " + image.Width);
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
            //Console.WriteLine("VisionServiceClient is created");

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

        static PriceRange GetPriceRange(string caption, Bitmap image, double confidence, float expensiveMultiplier)
        {
            PriceRange priceRange;
            priceRange.lowPrice = 0;
            priceRange.highPrice = 0;

            foreach ( char c in caption )
            {
                priceRange.highPrice += c;
            }

            System.Drawing.Color pixelSampleColor = image.GetPixel( image.Width / 2, image.Height / 2 );

            float red = (float)pixelSampleColor.R / 255.0f;
            float green = (float)pixelSampleColor.G / 255.0f;
            float blue = (float)pixelSampleColor.B / 255.0f;
            priceRange.highPrice *= (int)( 1.0f + red + green + blue );

            priceRange.highPrice = (int)(priceRange.highPrice * expensiveMultiplier);

            priceRange.highPrice *= 20; // General multiplier to inflate prices

            priceRange.lowPrice = (int)(priceRange.highPrice * confidence);

            return priceRange;
        }

        static int GetYear( Bitmap image, bool isOld )
        {
            int maxYear = 1917;
            int minYear = 500;

            if ( isOld )
            {
                // Make a guess that old stuff isn't modern but also isnt' ancient
                maxYear = 1900;
                minYear = 1000;
            }

            System.Drawing.Color pixelSampleColor = image.GetPixel( image.Width / 3, image.Height / 3 );

            float red = (float)pixelSampleColor.R / 255.0f;
            float green = (float)pixelSampleColor.G / 255.0f;
            float blue = (float)pixelSampleColor.B / 255.0f;
            
            float scale = (red + green + blue) / 3.0f;

            int year = (int)(minYear + (maxYear - minYear) * scale);
            return year;
        }

        static Appraisal CreateAppraisal( Bitmap sourceImage, AnalysisResult analysisResult )
        {
            Caption caption = GetCaption( analysisResult );
            Console.WriteLine( "Caption: " + caption.Text + " " + caption.Confidence );

            string foregroundColor = GetForegroundColor( analysisResult );
            string descriptionText = GetDescription( caption, foregroundColor );
            float confidence = (float)caption.Confidence;
            bool isOld = IsOld( analysisResult );
            float expensiveMultiplier = GetPriceExpensiveMultiplier( analysisResult );
            Console.WriteLine( "Is Old: " + isOld );

            Bitmap composedImage = ComposeImage( sourceImage, descriptionText, confidence, isOld, expensiveMultiplier );

            string comment = "";

            return new Appraisal( composedImage, comment );
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

        static bool IsOld( AnalysisResult analysisResult )
        {
            return analysisResult.Description.Tags.Contains( "old" );
        }

        static float GetPriceExpensiveMultiplier( AnalysisResult analysisResult )
        {
            string[] expensiveTags = {
                "gold",
                "silver",
                "decorated",
                "display",
                "large",
                "colorful",
                "old",
            };

            const float factor = 1.5f;
            float outMultiplier = 1.0f;

            foreach ( string tag in analysisResult.Description.Tags )
            {
                if ( expensiveTags.Contains( tag ) )
                {
                    outMultiplier *= factor;
                }
            }

            Console.WriteLine( "Expensive multiplier: " + outMultiplier );

            return outMultiplier;
        }

        static string GetDescription( Caption caption, string foregroundColor )
        {
            // Filter and adjust the caption
            string descriptionText = caption.Text;

            string[] stringsToRemove = {
                "a close up of ",
                " sitting on a table",
                " sitting on a counter",
                " on a table",
            };

            foreach (string text in stringsToRemove)
            {
                descriptionText = descriptionText.Replace(text, "");
            }

            // Capitalize the first letter
            descriptionText = char.ToUpper(descriptionText[0]) + descriptionText.Substring(1);

            string[] commonSimpleDescriptions = {
                "A vase",
                "A bowl",
                "A plate",
                "A knife",
                "A clock",
                "A cup of coffee",
            };

            bool isSimple = commonSimpleDescriptions.Contains( descriptionText );

            string color = foregroundColor.ToLower();

            if (isSimple)
            {
                descriptionText = descriptionText.Substring(0,2) + color + " " + descriptionText.Substring(2);
                Console.WriteLine("Added color to simple description: " + color);
            }

            return descriptionText;
        }

        static string GetForegroundColor( AnalysisResult analysisResult )
        {
            // If the foreground and background colors are the same, use the accent color.
            // Unfortunately the accent color is a hex string so we have to find the nearest
            // color that we know the name of.
            string color = "";
            if ( analysisResult.Color.DominantColorBackground == analysisResult.Color.DominantColorForeground )
            {
                color = ColorTable.GetClosestColorName( ColorTable.GetColorFromHexString( analysisResult.Color.AccentColor ) );
            }
            else
            {
                color = analysisResult.Color.DominantColorForeground;
            }

            return color;
        }

        static Bitmap ComposeImage(Bitmap sourceImage, string descriptionText, float confidence, bool isOld, float expensiveMultiplier)
        {
            Bitmap loadedBitmap = sourceImage;

            // There's some exception that's thrown when creating a Graphics from an "indexed bitmap"
            // which some of the images are. You have to create a new bitmap and that works.
            Bitmap drawnBitmap = new Bitmap( loadedBitmap );
            Graphics graphics = Graphics.FromImage(drawnBitmap);

            PriceRange priceRange = GetPriceRange( descriptionText, drawnBitmap, confidence, expensiveMultiplier );
            int year = GetYear( drawnBitmap, isOld );

            string fullCaption = descriptionText + String.Format( " (ca. {0})\n ${1:0,0}-${2:0,0}", year, priceRange.lowPrice, priceRange.highPrice);

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
