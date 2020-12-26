using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json;

using Microsoft.ProjectOxford.Vision.Contract;

using SixLabors.ImageSharp;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

using PixelColor = SixLabors.ImageSharp.PixelFormats.Rgba32;
using NamedColors = SixLabors.ImageSharp.Color;
using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    public class MetSearchResponse
    {
        public int total;
        public int[] objectIDs;
    }

    public class MetObjectResponse
    {
        public int objectID;
        public bool isPublicDomain;
        public string primaryImage;
        public string primaryImageSmall;
        public string[] additionalImages;
        public string title;
        public string rightsAndReproduction;
        public string objectURL;
    }

    class Appraisal
    {
        public Bitmap image;
        public string comment;

        public Appraisal(Bitmap inImage, string inComment)
        {
            image = inImage;
            comment = inComment;
        }
    }

    public class Program
    {
        public void awsLambdaHandler(Stream inputStream)
        {
            Console.WriteLine("starting via lambda");

            string argument = "create-and-post-appraisal-for-lambda";

            using (StreamReader reader = new StreamReader(inputStream, System.Text.Encoding.UTF8))
            {
                string inputString = reader.ReadToEnd();

                Console.WriteLine("Input: " + inputString);

                Newtonsoft.Json.Linq.JObject jsonObject = Newtonsoft.Json.Linq.JObject.Parse(inputString);

                if (jsonObject.ContainsKey("argument"))
                {
                    argument = jsonObject.Value<string>("argument");
                }
            }

            Console.WriteLine("Argument: " + argument);

            Main(new string[] { argument });
        }

        public static int Main(string[] executionArguments)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Beginning program. Arguments: " + string.Join(' ', executionArguments));

            if (executionArguments.Length == 0)
            {
                Console.WriteLine("Arguments need to be passed in.");
                return 1;
            }

            if (executionArguments[0] == "update-expected-acceptance-test-output")
            {
                UpdateExpectedAcceptanceTestOutput.Run();
            }
            else if (executionArguments[0] == "create-random-local-appraisals")
            {
                DeletePreviousOutput();

                CreateAppraisals(PostToTwitterMode.No);
            }
            else if (executionArguments[0] == "create-and-post-appraisal-for-lambda")
            {
                DeletePreviousOutput();

                InitializeTwitterCredentials();

                CreateAppraisals(PostToTwitterMode.Yes);
            }
            else if (executionArguments[0] == "test-load-every-file")
            {
                TestLoadEveryImage.Run();
            }
            else
            {
                Console.WriteLine("Arguments could not be matched to any handler.");
                return 1;
            }

            Console.WriteLine("Ending program successfully");
            return 0;
        }

        static void DeletePreviousOutput()
        {
            if (Directory.Exists("images"))
            {
                string[] filePaths = Directory.GetFiles(@"images\");
                foreach (string filePath in filePaths)
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (System.IO.IOException e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }

        static void InitializeTwitterCredentials()
        {
            string consumerKey = System.Environment.GetEnvironmentVariable("twitterConsumerKey");
            string consumerSecret = System.Environment.GetEnvironmentVariable("twitterConsumerSecret");
            string accessToken = System.Environment.GetEnvironmentVariable("twitterAccessToken");
            string accessTokenSecret = System.Environment.GetEnvironmentVariable("twitterAccessTokenSecret");

            if (consumerKey == null)
            {
                using (StreamReader fs = File.OpenText("localconfig/twitterKeys.txt"))
                {
                    consumerKey = fs.ReadLine();
                    consumerSecret = fs.ReadLine();
                    accessToken = fs.ReadLine();
                    accessTokenSecret = fs.ReadLine();
                }
            }

            Tweetinvi.Models.ITwitterCredentials credentials = Tweetinvi.Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);
            if (credentials is null)
            {
                Console.WriteLine("Twitter credentials not set.");
            }
        }

        enum PostToTwitterMode
        {
            Yes,
            No
        }

        static void CreateAppraisals(PostToTwitterMode postToTwitterMode)
        {
            int numItems = 1;

            Console.WriteLine("Getting collection listing");

            IEnumerable<MetObjectResponse> responseObjects = GetCollectionListing(numItems);

            Console.WriteLine("Found " + responseObjects.Count() + " results");

            int objectCounter = 0;
            foreach (MetObjectResponse responseObject in responseObjects)
            {
                Console.WriteLine("-----------------------------------------------------------------------");

                string imageUrl = responseObject.primaryImage;
                Console.WriteLine("image url: " + imageUrl);

                string fullListingURL = responseObject.objectURL;
                Console.WriteLine("Listing page: " + fullListingURL);

                Bitmap originalImage = DownloadImage(imageUrl);

                if (originalImage != null)
                {
                    Bitmap resizedImage = ImageTransforms.ResizeToWithinAnalysisLimits(originalImage);

                    if (Directory.Exists("images"))
                    {
                        string originalDestinationFilePath = @"images/sourceImage" + objectCounter + ".jpg";
                        originalImage.Save(originalDestinationFilePath);

                        string resizedDestinationFilePath = @"images/resizedImage" + objectCounter + ".jpg";
                        resizedImage.Save(resizedDestinationFilePath);
                    }

                    ComputerVisionService.AnalysisBlob analysisBlob = ComputerVisionService.GetAnalysisBlob(resizedImage);

                    string tagString = "";
                    foreach (string tag in analysisBlob.generalAnalysisResult.Description.Tags)
                    {
                        tagString += tag + ", ";
                    }
                    Console.WriteLine("Tags: " + tagString);

                    string accentColor = ColorTable.GetClosestColorName(analysisBlob.generalAnalysisResult.Color.AccentColor);

                    Console.WriteLine("Foreground: " + analysisBlob.generalAnalysisResult.Color.DominantColorForeground + " Background: " + analysisBlob.generalAnalysisResult.Color.DominantColorBackground + " Accent: " + accentColor);

                    if (analysisBlob.generalAnalysisResult.Categories != null)
                    {
                        string categoryString = "";
                        foreach (Category category in analysisBlob.generalAnalysisResult.Categories)
                        {
                            categoryString += category.Name + ", ";
                        }
                        Console.WriteLine("Categories: " + categoryString);
                    }

                    Appraisal appraisal = CreateAppraisal(originalImage, analysisBlob);

                    if (Directory.Exists("images"))
                    {
                        string destinationFilePath = @"images/finalImage" + objectCounter + ".jpg";
                        appraisal.image.Save(destinationFilePath);

                        using (StreamWriter file = File.CreateText(@"images/comment" + objectCounter + ".txt"))
                        {
                            file.WriteLine(appraisal.comment);
                        }
                    }

                    if (postToTwitterMode == PostToTwitterMode.Yes)
                    {
                        TweetAppraisal(appraisal);
                    }
                }

                objectCounter++;
            }
        }

        static string GetMetSearchAPIUrl(string material)
        {
            return "https://collectionapi.metmuseum.org/public/collection/v1/search"
            + "?medium=" + material
            + "&q=" + material // The endpoint doesn't return anything if q is not supplied.
            + "&hasImages=true";
        }

        static string GetMetObjectAPIUrl(int objectID)
        {
            return "https://collectionapi.metmuseum.org/public/collection/v1/objects/" + objectID;
        }

        static IEnumerable<MetObjectResponse> GetCollectionListing(int numItems)
        {
            Random random = new Random();
            IEnumerable<int> randomSetOfObjectIDs = GetRandomObjectIDs(numItems, random);

            return randomSetOfObjectIDs.Select(objectID => GetObjectResponse(objectID));
        }

        static MetObjectResponse GetObjectResponse(int objectID)
        {
            return GetWebResponse<MetObjectResponse>(GetMetObjectAPIUrl(objectID));
        }

        static IEnumerable<int> GetRandomObjectIDs(int numItems, Random random)
        {
            BNolan.RandomSelection.Selector<string> materialSelector = new BNolan.RandomSelection.Selector<string>();

            // Weights come from how many items fit the search criteria. This can be found by hitting:
            // https://collectionapi.metmuseum.org/public/collection/v1/search?medium=Furniture&q=Furniture&hasImages=true
            // and replacing Furniture with the name of the material.
            // For scale, there are around 470,000 total objects.
            materialSelector.TryAddItem("Bags", "Bags", 3);
            materialSelector.TryAddItem("Jewelry", "Jewelry", 17);
            materialSelector.TryAddItem("Sculpture", "Sculpture", 232);
            materialSelector.TryAddItem("Bowls", "Bowls", 19);
            materialSelector.TryAddItem("Furniture", "Furniture", 66);
            materialSelector.TryAddItem("Musical%20instruments", "Musical%20instruments", 6);
            materialSelector.TryAddItem("Vessels", "Vessels", 41);
            materialSelector.TryAddItem("Ceramics", "Ceramics", 66);
            materialSelector.TryAddItem("Wood", "Wood", 110);
            materialSelector.TryAddItem("Paintings", "Paintings", 491);
            materialSelector.TryAddItem("Timepieces", "Timepieces", 2);
            materialSelector.TryAddItem("Arms", "Arms", 7);
            materialSelector.TryAddItem("Costume", "Costume", 42);
            materialSelector.TryAddItem("Cases", "Cases", 10);
            materialSelector.TryAddItem("Metal", "Metal", 384);
            materialSelector.TryAddItem("Lithographs", "Lithographs", 55);
            materialSelector.TryAddItem("Prints", "Prints", 352);
            materialSelector.TryAddItem("Silk", "Silk", 91);

            string material = materialSelector.RandomSelect(1).First().Value;

            string searchURL = GetMetSearchAPIUrl(material);

            MetSearchResponse response = GetWebResponse<MetSearchResponse>(searchURL);

            Console.WriteLine("Total items in category: " + response.total);

            Console.WriteLine("Material: " + material + " numItems: " + numItems);

            if (numItems > response.total)
            {
                throw new Exception("Not enough items meet search criteria. Requested: " + numItems + " Found: " + response.total);
            }

            return RandomSubset(response.objectIDs, numItems, random);
        }

        static T GetWebResponse<T>(string url)
        {
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().GetAwaiter().GetResult())
            {
                using (StreamReader readStream = new StreamReader(response.GetResponseStream(), encode))
                {
                    string responseText = readStream.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(responseText);
                }
            }
        }

        static Bitmap DownloadImage(string url)
        {
            try
            {
                HttpWebRequest lxRequest = (HttpWebRequest)WebRequest.Create(url);

                // returned values are returned as a stream, then read into a string
                using (HttpWebResponse lxResponse = (HttpWebResponse)lxRequest.GetResponseAsync().GetAwaiter().GetResult())
                {

                    Bitmap image = Image.Load<PixelColor>(lxResponse.GetResponseStream());

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

        static PriceRange GetPriceRange(string caption, double confidence, float expensiveMultiplier, Random rnd)
        {
            PriceRange priceRange;
            priceRange.lowPrice = 0;
            priceRange.highPrice = 0;

            // Longer captions means higher prices
            foreach (char c in caption)
            {
                priceRange.highPrice += c;
            }

            // Randomly increase price
            priceRange.highPrice *= (int)(1.0f + rnd.NextDouble() * 30.0f);

            priceRange.highPrice = (int)(priceRange.highPrice * expensiveMultiplier);

            priceRange.highPrice *= 2; // General multiplier to inflate prices

            priceRange.lowPrice = (int)(priceRange.highPrice * confidence);

            PriceRange roundedRange = PriceRange.RoundPrices(priceRange);

            return roundedRange;
        }

        static int GetYear(Bitmap image, bool isOld, bool isBlackAndWhitePhoto, int? extractedYear)
        {
            if (extractedYear.HasValue)
            {
                return extractedYear.Value;
            }

            int maxYear = 1917;
            int minYear = 500;

            if (isOld && isBlackAndWhitePhoto)
            {
                maxYear = 1900;
                minYear = 1830;
            }
            else if (isBlackAndWhitePhoto)
            {
                // Estimate of the timeline of black and white photos
                maxYear = 1930;
                minYear = 1830;
            }
            else if (isOld)
            {
                // Make a guess that old stuff isn't modern but also isn't ancient
                maxYear = 1900;
                minYear = 1000;
            }

            PixelColor pixelSampleColor = image[image.Width / 3, image.Height / 3];

            float red = (float)pixelSampleColor.R / 255.0f;
            float green = (float)pixelSampleColor.G / 255.0f;
            float blue = (float)pixelSampleColor.B / 255.0f;

            float scale = (red + green + blue) / 3.0f;

            int year = (int)(minYear + (maxYear - minYear) * scale);
            return year;
        }

        static Appraisal CreateAppraisal(Bitmap sourceImage, ComputerVisionService.AnalysisBlob analysisResult)
        {
            Caption caption = GetCaption(analysisResult.generalAnalysisResult);
            Console.WriteLine("Caption: " + caption.Text + " " + caption.Confidence);

            string foregroundColor = GetForegroundColor(analysisResult.generalAnalysisResult);
            float confidence = (float)caption.Confidence;
            bool isOld = IsOld(analysisResult.generalAnalysisResult);
            float expensiveMultiplier = GetPriceExpensiveMultiplier(analysisResult.generalAnalysisResult);
            Console.WriteLine("Is Old: " + isOld);
            bool isBlackAndWhite = IsBlackAndWhite(analysisResult.generalAnalysisResult);
            Console.WriteLine("Is Black and White: " + isBlackAndWhite);

            Description.Arguments descriptionArguments = new Description.Arguments{
                foregroundColor = foregroundColor,
                isOld = isOld,
                isBlackAndWhite = isBlackAndWhite,
            };

            string descriptionText = Description.Get(caption, descriptionArguments);
            Console.WriteLine("Final Description Text: " + descriptionText);
            bool isPainting = PaintingDetection.IsPainting(analysisResult);
            Console.WriteLine("Is Painting: " + isPainting);
            bool isPhoto = !isPainting && IsPhoto(analysisResult.generalAnalysisResult);
            Console.WriteLine("Is Photo: " + isPhoto);
            bool isSign = SignDetection.IsSign(analysisResult);
            Console.WriteLine("Is Sign: " + isSign);
            int? extractedYear = YearExtractor.ExtractYear(analysisResult.ocrAnalysisResult);
            Console.WriteLine("Extracted Year: " + extractedYear);
            string extractedLocale = LanguageCodeToLocation.LookUp(analysisResult.ocrAnalysisResult);
            Console.WriteLine("Extracted Locale: " + extractedLocale);

            Random random = GetDeterministicRandom(sourceImage);
            Bitmap composedImage = ComposeImage(sourceImage, descriptionText, confidence, isOld, isBlackAndWhite && isPhoto, expensiveMultiplier, isPainting, isSign, extractedYear, extractedLocale, random);

            return new Appraisal(composedImage, descriptionText);
        }

        public static Random GetDeterministicRandom(Bitmap image)
        {
            return new Random(image.ToBase64String(SixLabors.ImageSharp.Formats.Png.PngFormat.Instance).GetHashCode());
        }

        static Caption GetCaption(AnalysisResult analysisResult)
        {
            Caption caption = new Caption();
            caption.Text = "Something";
            caption.Confidence = 0.0001f;

            foreach (Caption c in analysisResult.Description.Captions)
            {
                if (c.Confidence > caption.Confidence)
                {
                    caption = c;
                }
            }

            return caption;
        }

        static bool IsOld(AnalysisResult analysisResult)
        {
            string[] oldTags = {
                "old",
                "vintage",
            };

            foreach (string tag in analysisResult.Description.Tags)
            {
                if (oldTags.Contains(tag))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsBlackAndWhite(AnalysisResult analysisResult)
        {
            if (analysisResult.Description.Tags.Contains("photo")
            && analysisResult.Description.Tags.Contains("vintage"))
            {
                // vintage photo means a black and white photo. Black and white photos are black and white.
                return true;
            }

            return analysisResult.IsBlackAndWhite();
        }

        static bool IsPhoto(AnalysisResult analysisResult)
        {
            if (analysisResult.Description.Tags.Contains("photo"))
            {
                return true;
            }

            return !analysisResult.IsClipArt()
            && !analysisResult.IsLineDrawing();
        }

        static bool HasCelebrities(ComputerVisionService.CelebrityAnalysisResult celebrityResult)
        {
            return celebrityResult.celebrities.Count > 0;
        }

        static float GetPriceExpensiveMultiplier(AnalysisResult analysisResult)
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

            foreach (string tag in analysisResult.Description.Tags)
            {
                if (expensiveTags.Contains(tag))
                {
                    outMultiplier *= factor;
                }
            }

            Console.WriteLine("Expensive multiplier: " + outMultiplier);

            return outMultiplier;
        }


        static string GetForegroundColor(AnalysisResult analysisResult)
        {
            // If the foreground and background colors are the same, use the accent color.
            // Unfortunately the accent color is a hex string so we have to find the nearest
            // color that we know the name of.
            string color = "";
            if (analysisResult.Color.DominantColorBackground == analysisResult.Color.DominantColorForeground)
            {
                color = ColorTable.GetClosestColorName(analysisResult.Color.AccentColor);
            }
            else
            {
                color = analysisResult.Color.DominantColorForeground;
            }

            return color;
        }

        public static Bitmap ComposeImage(Bitmap sourceImage, string descriptionText, float confidence, bool isOld, bool isBlackAndWhitePhoto, float expensiveMultiplier, bool isPainting, bool isSign, int? extractedYear, string extractedLocale, Random random)
        {
            Bitmap drawnBitmap = null;

            if (isPainting || isSign)
            {
                drawnBitmap = ImageTransforms.ComposeImageOntoPhoto(sourceImage);
            }
            else
            {
                drawnBitmap = sourceImage.Clone();
            }

            PriceRange priceRange = GetPriceRange(descriptionText, confidence, expensiveMultiplier, random);
            int year = GetYear(drawnBitmap, isOld, isBlackAndWhitePhoto, extractedYear);

            string localePhrase = ( extractedLocale != null ? (", " + extractedLocale ) : "" );

            string fullCaption = descriptionText + String.Format(" (ca. {0}{1})\n ${2}-${3}", year, localePhrase, PriceRange.FormatPrice(priceRange.lowPrice), PriceRange.FormatPrice(priceRange.highPrice));

            Bitmap footerImage = LoadImage(LoadImageType.Source, "footer.png");

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

            TextGraphicsOptions textGraphicsOptions = new TextGraphicsOptions();
            textGraphicsOptions.TextOptions.WrapTextWidth = drawnBitmap.Width - textOriginX - 10;

            AffineTransformBuilder footerTransformBuilder = new AffineTransformBuilder()
                    .AppendScale(new SizeF(scale, scale));

            footerImage.Mutate(x => x.Transform(footerTransformBuilder));

            drawnBitmap.Mutate(x => x.DrawImage(footerImage, new SixLabors.ImageSharp.Point(0, (int)footerOriginY), new GraphicsOptions())
           .DrawText(textGraphicsOptions, fullCaption, font, NamedColors.White, new PointF(textOriginX + 1, textOriginY + 1)));

            return drawnBitmap;
        }

        static void TweetAppraisal(Appraisal appraisal)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                appraisal.image.SaveAsPng(memoryStream);
                byte[] bytes = memoryStream.ToArray();

                Console.WriteLine("Uploading image to twitter");
                Tweetinvi.Models.IMedia media = Tweetinvi.Upload.UploadBinary(new Tweetinvi.Parameters.UploadParameters { Binary = bytes });

                Console.WriteLine("Publishing tweet");
                Tweetinvi.Models.ITweet tweet = Tweetinvi.Tweet.PublishTweet(appraisal.comment, new Tweetinvi.Parameters.PublishTweetOptionalParameters
                {
                    Medias = new List<Tweetinvi.Models.IMedia> { media }
                });
                Console.WriteLine("Tweet published to: " + tweet.Url);
            }
        }

        static IEnumerable<T> RandomSubset<T>(IEnumerable<T> originalList, int numberOfItemsToTake, Random rnd)
        {
            if (numberOfItemsToTake > originalList.Count())
            {
                throw new Exception("Not enough items to take subset. Requested: " + numberOfItemsToTake + " Have: " + originalList.Count());
            }

            return originalList.OrderBy(x => rnd.NextDouble()).Take(numberOfItemsToTake);
        }

        public enum LoadImageType
        {
            Source,
            Test
        }

        public static Bitmap LoadImage(LoadImageType type, string fileName)
        {
            string directory = type == LoadImageType.Source ? "sourceArt" : "testArt";

            Console.WriteLine("LoadImage: " + fileName);

            if (Program.IsRunningTests())
            {
                return Image.Load<PixelColor>("../../../" + directory + "/" + fileName);
            }
            else
            {
                if (Directory.Exists(directory))
                {
                    return Image.Load<PixelColor>(directory + "/" + fileName);
                }
                else
                {
                    Amazon.S3.AmazonS3Client client = new Amazon.S3.AmazonS3Client(Amazon.RegionEndpoint.USEast2);
                    Amazon.S3.Model.GetObjectResponse response = client.GetObjectAsync("appraisal-bot", fileName).GetAwaiter().GetResult();
                    return Image.Load<PixelColor>(response.ResponseStream);
                }
            }
        }

        public static bool IsRunningTests()
        {
            return Assembly.GetEntryAssembly().GetName().Name.Contains("test");
        }


    }
}
