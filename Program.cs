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

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.Fonts;
using SixLabors.Primitives;

using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.Rgba32>;

namespace AppraisalBot
{
    public class MetResponse
    {
        public class MetResult
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

    class AnalysisBlob
    {
        public AnalysisResult generalAnalysisResult;
        public CelebrityAnalysisResult celebrityAnalysisResult;
    }

    public class Program
    {
        static string computerVisionKey = "";
        public Stream awsLambdaHandler(Stream inputStream)
       {
           //Main(new string[0]);
           Console.WriteLine("starting via lambda");
           Main( new string[0]);
           return inputStream;
       }
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Beginning program");

            // Delete the previous output
            if ( Directory.Exists("images") )
            {
                string[] filePaths = Directory.GetFiles(@"images\");
                foreach (string filePath in filePaths)
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch ( System.IO.IOException e )
                    {
                        Console.WriteLine( e.ToString() );
                    }
                }
            }

            computerVisionKey = System.Environment.GetEnvironmentVariable ("computerVisionKey");

            if (computerVisionKey == null)
            {
                using ( StreamReader fs = File.OpenText( "localconfig/computervisionkey.txt" ) )
                {
                    computerVisionKey = fs.ReadToEnd();
                }
            }

            string consumerKey = System.Environment.GetEnvironmentVariable ("twitterConsumerKey");
            string consumerSecret = System.Environment.GetEnvironmentVariable ("twitterConsumerSecret");
            string accessToken = System.Environment.GetEnvironmentVariable ("twitterAccessToken");
            string accessTokenSecret = System.Environment.GetEnvironmentVariable ("twitterAccessTokenSecret");

            if (consumerKey == null)
            {
                using ( StreamReader fs = File.OpenText( "localconfig/twitterKeys.txt" ) )
                {
                    consumerKey = fs.ReadLine();
                    consumerSecret = fs.ReadLine();
                    accessToken = fs.ReadLine();
                    accessTokenSecret = fs.ReadLine();
                }
            }

            Tweetinvi.Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);

            int numItems = 1;

            Console.WriteLine("Getting collection listing");

            MetResponse responseObject = GetCollectionListing( numItems );

            Console.WriteLine( "Found " + responseObject.results.Count + " results" );

            for ( int i = 0; i < responseObject.results.Count; i++ )
            {
                Console.WriteLine("-----------------------------------------------------------------------");

                //Console.WriteLine("small url: " + responseObject.results[i].image);
                //Console.WriteLine("large url: " + responseObject.results[i].largeImage );

                string smallImageUrl = responseObject.results[i].image;
                int index = smallImageUrl.LastIndexOf( responseObject.results[i].largeImage.Substring(0,3) );
                string largeImageUrl = smallImageUrl.Substring(0,index) + responseObject.results[i].largeImage;
                Console.WriteLine("large url: " + largeImageUrl);
                
                string fullListingURL = "https://www.metmuseum.org" + responseObject.results[i].url;
                int questionMarkIndex = fullListingURL.LastIndexOf('?');
                string shortenedURL = fullListingURL.Substring(0,questionMarkIndex);
                Console.WriteLine("Listing page: " + shortenedURL);

                Bitmap image = DownloadImage( largeImageUrl );

                bool doAnalysis = true;
                if (image != null && doAnalysis)
                {
                    if ( Directory.Exists("images"))
                    {
                        string destinationFilePath = @"images/sourceImage" + i + ".jpg";
                        image.Save( destinationFilePath );
                    }

                    AnalysisBlob analysisBlob = new AnalysisBlob();
                    analysisBlob.generalAnalysisResult = AnalyzeImage( image );
                    analysisBlob.celebrityAnalysisResult = AnalyzeImageForCelebrities( image );

                    string tagString = "";
                    foreach ( string tag in analysisBlob.generalAnalysisResult.Description.Tags )
                    {
                        tagString += tag + ", ";
                    }
                    Console.WriteLine( "Tags: " + tagString );

                    string accentColor = ColorTable.GetClosestColorName( ColorTable.GetColorFromHexString( analysisBlob.generalAnalysisResult.Color.AccentColor ) );

                    Console.WriteLine("Foreground: " + analysisBlob.generalAnalysisResult.Color.DominantColorForeground + " Background: " + analysisBlob.generalAnalysisResult.Color.DominantColorBackground + " Accent: " + accentColor );

                    if ( analysisBlob.generalAnalysisResult.Categories != null )
                    {
                        string categoryString = "";
                        foreach ( Category category in analysisBlob.generalAnalysisResult.Categories )
                        {
                            categoryString += category.Name + ", ";
                        }
                        Console.WriteLine("Categories: " + categoryString);
                    }

                    if ( HasCelebrities( analysisBlob.celebrityAnalysisResult ) )
                    {
                        string celebrityString = "";
                        foreach ( CelebrityAnalysisResult.Celebrity celebrity in analysisBlob.celebrityAnalysisResult.celebrities )
                        {
                            celebrityString += celebrity.name + ", ";
                        }
                        Console.WriteLine("Celebrities: " + celebrityString);
                    }

                    Appraisal appraisal = CreateAppraisal( image, analysisBlob );

                    if ( Directory.Exists("images"))
                    {
                        string destinationFilePath = @"images/finalImage" + i + ".jpg";
                        appraisal.image.Save( destinationFilePath );

                        using (StreamWriter file = File.CreateText(@"images/comment" + i + ".txt") )
                        {
                            file.WriteLine(appraisal.comment);
                        }
                    }

                    TweetAppraisal( appraisal );
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

            using ( HttpWebResponse categorySizeResponse = (HttpWebResponse)categorySizeRequest.GetResponseAsync().GetAwaiter().GetResult() )
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

            using ( HttpWebResponse response = (HttpWebResponse)myReq.GetResponseAsync().GetAwaiter().GetResult() )
            {
                using ( StreamReader readStream = new StreamReader( response.GetResponseStream(), encode ) )
                {
                    string responseText = readStream.ReadToEnd();
                    MetResponse responseObject = JsonConvert.DeserializeObject<MetResponse>(responseText);
                    return responseObject;
                }
            }
        }

        static Bitmap DownloadImage(string url)
        {
            try 
            {
                HttpWebRequest lxRequest = (HttpWebRequest)WebRequest.Create(url);

                // returned values are returned as a stream, then read into a string
                using (HttpWebResponse lxResponse = (HttpWebResponse)lxRequest.GetResponseAsync().GetAwaiter().GetResult()){
                    
                    Bitmap image = Image.Load( lxResponse.GetResponseStream() );

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
                Console.WriteLine("exception thrown during get for " + url + " " + e);
            }
            return null;
        }

        static AnalysisResult AnalyzeImage(Bitmap sourceImage)
        {
            VisionServiceClient VisionServiceClient = new VisionServiceClient(computerVisionKey, "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");

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

                AnalysisResult analysisResult = VisionServiceClient.AnalyzeImageAsync( memoryStream, visualFeatures).GetAwaiter().GetResult();
                return analysisResult;
            }
        }

        static Bitmap SmartCropImage(Bitmap sourceImage)
        {
            VisionServiceClient VisionServiceClient = new VisionServiceClient(computerVisionKey, "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                sourceImage.SaveAsPng(memoryStream);
                memoryStream.Position = 0;

                int width = sourceImage.Width;
                int height = (int)(sourceImage.Width / 16.0f * 9.0f * 1.2f); // Set it to a 16:9 with an extra 20% to increase the overall size
                Console.WriteLine("Original Width: " + sourceImage.Width + " Original Height: " + sourceImage.Height + " Cropped Height: " + height);

                if ( sourceImage.Height > height )
                {
                    Console.WriteLine("Calling VisionServiceClient.GetThumbnailAsync()...");
                    byte[] bytes = VisionServiceClient.GetThumbnailAsync( memoryStream, width, height ).GetAwaiter().GetResult();

                    Bitmap croppedImage = Image.Load( bytes );

                    return croppedImage;
                }
                else
                {
                    Console.WriteLine("Image was already small. No reason to crop");
                    return sourceImage;
                }
            }
        }

        static CelebrityAnalysisResult AnalyzeImageForCelebrities(Bitmap sourceImage)
        {
            VisionServiceClient VisionServiceClient = new VisionServiceClient(computerVisionKey, "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                sourceImage.SaveAsPng(memoryStream);
                memoryStream.Position = 0;

                Console.WriteLine("Calling VisionServiceClient.AnalyzeImageInDomainAsync()...");

                // This is how you'd recognize celebrities like Henry Clay
                Microsoft.ProjectOxford.Vision.Contract.AnalysisInDomainResult result = VisionServiceClient.AnalyzeImageInDomainAsync( memoryStream, "celebrities" ).GetAwaiter().GetResult();

                Newtonsoft.Json.Linq.JObject jsonObj = result.Result as Newtonsoft.Json.Linq.JObject;

                CelebrityAnalysisResult celebResult = jsonObj.ToObject<CelebrityAnalysisResult>() as CelebrityAnalysisResult;
                return celebResult;
            }
        }

        static PriceRange GetPriceRange(string caption, double confidence, float expensiveMultiplier)
        {
            PriceRange priceRange;
            priceRange.lowPrice = 0;
            priceRange.highPrice = 0;

            // Longer captions means higher prices
            foreach ( char c in caption )
            {
                priceRange.highPrice += c;
            }

            // Randomly increase price
            Random rnd = new Random();
            priceRange.highPrice *= (int)( 1.0f + rnd.NextDouble() * 3.0 );

            priceRange.highPrice = (int)(priceRange.highPrice * expensiveMultiplier);

            priceRange.highPrice *= 20; // General multiplier to inflate prices

            priceRange.lowPrice = (int)(priceRange.highPrice * confidence);

            return priceRange;
        }

        static int GetYear( Bitmap image, bool isOld, bool isBlackAndWhitePhoto )
        {
            int maxYear = 1917;
            int minYear = 500;

            if ( isOld && isBlackAndWhitePhoto )
            {
                maxYear = 1900;
                minYear = 1830;
            }
            else if ( isBlackAndWhitePhoto )
            {
                // Estimate of the timeline of black and white photos
                maxYear = 1930;
                minYear = 1830;
            }
            else if ( isOld )
            {
                // Make a guess that old stuff isn't modern but also isn't ancient
                maxYear = 1900;
                minYear = 1000;
            }

            Rgba32 pixelSampleColor = image[ image.Width / 3, image.Height / 3 ];

            float red = (float)pixelSampleColor.R / 255.0f;
            float green = (float)pixelSampleColor.G / 255.0f;
            float blue = (float)pixelSampleColor.B / 255.0f;
            
            float scale = (red + green + blue) / 3.0f;

            int year = (int)(minYear + (maxYear - minYear) * scale);
            return year;
        }

        static Appraisal CreateAppraisal( Bitmap sourceImage, AnalysisBlob analysisResult )
        {
            Caption caption = GetCaption( analysisResult.generalAnalysisResult );
            Console.WriteLine( "Caption: " + caption.Text + " " + caption.Confidence );

            string foregroundColor = GetForegroundColor( analysisResult.generalAnalysisResult );
            float confidence = (float)caption.Confidence;
            bool isOld = IsOld( analysisResult.generalAnalysisResult );
            float expensiveMultiplier = GetPriceExpensiveMultiplier( analysisResult.generalAnalysisResult );
            Console.WriteLine( "Is Old: " + isOld );
            bool isBlackAndWhite = IsBlackAndWhite( analysisResult.generalAnalysisResult );
            Console.WriteLine("Is Black and White: " + isBlackAndWhite );
            string descriptionText = GetDescription( caption, foregroundColor, isOld, isBlackAndWhite );
            Console.WriteLine("Final Description Text: " + descriptionText);
            bool isPainting = PaintingDetection.IsPainting( analysisResult );
            Console.WriteLine("Is Painting: " + isPainting);
            bool isPhoto = !isPainting && IsPhoto( analysisResult.generalAnalysisResult );
            Console.WriteLine("Is Photo: " + isPhoto);
            bool hasCelebrities = HasCelebrities( analysisResult.celebrityAnalysisResult );
            Console.WriteLine("Has Celebrities: " + hasCelebrities);
            bool isSign = SignDetection.IsSign( analysisResult );
            Console.WriteLine("Is Sign: " + isSign);

            Bitmap composedImage = ComposeImage( sourceImage, descriptionText, confidence, isOld, isBlackAndWhite && isPhoto, expensiveMultiplier, isPainting, isSign );

            //string comment = Comment.Get();

            return new Appraisal( composedImage, descriptionText );
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
            string[] oldTags = {
                "old",
                "vintage",
            };

            foreach (string tag in analysisResult.Description.Tags )
            {
                if ( oldTags.Contains( tag ) )
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsBlackAndWhite( AnalysisResult analysisResult )
        {
            if ( analysisResult.Description.Tags.Contains( "photo")
            && analysisResult.Description.Tags.Contains("vintage") )
            {
                // vintage photo means a black and white photo. Black and white photos are black and white.
                return true;
            }

            return analysisResult.IsBlackAndWhite();
        }

        static bool IsPhoto( AnalysisResult analysisResult )
        {
            if ( analysisResult.Description.Tags.Contains( "photo") )
            {
                return true;
            }

            return !analysisResult.IsClipArt()
            && !analysisResult.IsLineDrawing();
        }

        static bool HasCelebrities( CelebrityAnalysisResult celebrityResult )
        {
            return celebrityResult.celebrities.Count > 0;
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
                "vintage",
                "artifact",
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

        static string GetDescription( Caption caption, string foregroundColor, bool isOld, bool isBlackAndWhite )
        {
            // Filter and adjust the caption
            string descriptionText = caption.Text;

            string[] stringsToRemove = {
                "a close up of ",
                " that is sitting on a table",
                " sitting on a table",
                " sitting on a counter",
                " on a table",
                "a vintage photo of ",
                " sitting on top of a table",
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
                "A bird",
                "A tool",
                "A weapon",
                "A gun",
                "A logo",
                "A box",
                "A sign",
                "A envelope",
                "A pot",
            };

            bool isSimple = commonSimpleDescriptions.Contains( descriptionText );

            if (isSimple)
            {
                if (isBlackAndWhite)
                {
                    if (isOld)
                    {
                        descriptionText = "An old " + descriptionText.Substring(2);
                        Console.WriteLine("Added 'old' to simple description");
                    }
                    else
                    {
                        // TODO: Do something clever here?
                        // I don't want to add a color here since I know the image is black and white.
                        // This might be pretty rare since black and white images are often old
                        Console.WriteLine("Description was simple but not old. Leaving simple description.");
                    }
                }
                else
                {
                    string color = foregroundColor.ToLower();
                    descriptionText = descriptionText.Substring(0,2) + color + " " + descriptionText.Substring(2);
                    Console.WriteLine("Added color to simple description: " + color);
                }
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

        static Bitmap ComposeImage(Bitmap sourceImage, string descriptionText, float confidence, bool isOld, bool isBlackAndWhitePhoto, float expensiveMultiplier, bool isPainting, bool isSign)
        {
            Bitmap drawnBitmap = null;
            
            if ( isPainting || isSign )
            {
                drawnBitmap = ImageTransforms.ComposeImageOntoPhoto( sourceImage );
            }
            else
            {
                drawnBitmap = sourceImage.Clone();
            }

            PriceRange priceRange = GetPriceRange( descriptionText, confidence, expensiveMultiplier );
            int year = GetYear( drawnBitmap, isOld, isBlackAndWhitePhoto );

            string fullCaption = descriptionText + String.Format( " (ca. {0})\n ${1:0,0}-${2:0,0}", year, priceRange.lowPrice, priceRange.highPrice);

            Bitmap footerImage = null;
            
            if ( Directory.Exists("sourceArt" ) )
            {
                footerImage = Image.Load(@"sourceArt/footer.png");
            } 
            else
            {
                Amazon.S3.AmazonS3Client client = new Amazon.S3.AmazonS3Client( Amazon.RegionEndpoint.USEast2 );
                Amazon.S3.Model.GetObjectResponse response = client.GetObjectAsync( "appraisal-bot", "footer.png" ).GetAwaiter().GetResult();
                footerImage = Image.Load( response.ResponseStream );
            }

            float scale = (float)drawnBitmap.Width / (float)footerImage.Width;
            float footerHeight = scale * footerImage.Height;
            float footerOriginY = drawnBitmap.Height - footerHeight;

            float textOriginY = footerOriginY + 10.0f * scale;
            float textOriginX = 200.0f * scale;

            int fontSize = (int)(33 * scale);

            // System.Collections.Generic.IEnumerable<FontFamily> families = SystemFonts.Families;
            // IOrderedEnumerable<FontFamily> orderd = families.OrderBy(x => x.Name);
            // int len = families.Max(x => x.Name.Length);
            // foreach (FontFamily f in orderd)
            // {
            //     Console.Write(f.Name.PadRight(len));
            //     Console.Write('\t');
            //     Console.Write(string.Join(",", f.AvailibleStyles.OrderBy(x=>x).Select(x => x.ToString())));
            //     Console.WriteLine();
            // }

            FontFamily family = SystemFonts.Find("DejaVu Sans"); //assumes arial has been installed
            Font font = new Font(family, fontSize, FontStyle.Bold);

            drawnBitmap.Mutate( x => x.DrawImage( footerImage, new Size(drawnBitmap.Width, (int)footerHeight), new SixLabors.Primitives.Point( 0, (int)footerOriginY), new GraphicsOptions() )
            .DrawText( fullCaption, font, Rgba32.White, new PointF( textOriginX + 1, textOriginY + 1 ) ) );

            return drawnBitmap;
        }
        static void TweetAppraisal( Appraisal appraisal )
        {
            using ( MemoryStream memoryStream = new MemoryStream() )
            {
                appraisal.image.SaveAsPng(memoryStream);
                byte[] bytes = memoryStream.ToArray();

                Console.WriteLine("Uploading image to twitter");
                var media = Tweetinvi.Upload.UploadImage(bytes);

                Console.WriteLine("Publishing tweet");
                var tweet = Tweetinvi.Tweet.PublishTweet(appraisal.comment, new Tweetinvi.Parameters.PublishTweetOptionalParameters
                {
                    Medias = new List<Tweetinvi.Models.IMedia> { media }
                });
            }
        }
    }
}
