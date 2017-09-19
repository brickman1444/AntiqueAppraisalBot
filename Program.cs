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

            int numItems = 5;

            Random rnd = new Random();
            int collectionOffset = rnd.Next(0,1950);

            string responseText = GetCollectionListing( numItems, collectionOffset );
            MetResponse responseObject = JsonConvert.DeserializeObject<MetResponse>(responseText);

            Console.WriteLine( "Found " + responseObject.results.Count + " results" );

            for ( int i = 0; i < responseObject.results.Count; i++ )
            {
                string fileLocation = "images/image" + i + ".jpg";
                bool success = DownloadImage(responseObject.results[i].image, fileLocation );
                bool doAnalysis = true;
                if (success && doAnalysis)
                {
                    AnalysisResult analysisResult = AnalyzeImage( fileLocation ).GetAwaiter().GetResult();

                    foreach ( Caption caption in analysisResult.Description.Captions )
                    {
                        Console.WriteLine( "Caption: " + caption.Text + " " + caption.Confidence );
                    }

                    foreach ( string tag in analysisResult.Description.Tags )
                    {
                        Console.WriteLine( "Tag: " + tag );
                    }
                    
                    if ( analysisResult.Categories != null )
                    {
                        foreach ( Category category in analysisResult.Categories )
                        {
                            Console.WriteLine( "Category: " + category.Name + " " + category.Score);
                        }
                    }

                    Bitmap loadedBitmap = (Bitmap)Image.FromFile(fileLocation);

                    // There's some exception that's thrown when creating a Graphics from an "indexed bitmap"
                    // which some of the images are. You have to create a new bitmap and that works.
                    Bitmap drawnBitmap = new Bitmap( loadedBitmap );
                    Graphics graphics = Graphics.FromImage(drawnBitmap);

                    string descriptionText = analysisResult.Description.Captions[0].Text;
                    descriptionText = descriptionText.Replace("a close up of ", "");
                    descriptionText = descriptionText.Replace(" sitting on a table", "");
                    descriptionText = descriptionText.Replace(" on a table", "");

                    PriceRange priceRange = GetPriceRange( descriptionText, drawnBitmap, analysisResult.Description.Captions[0].Confidence );

                    string fullCaption = descriptionText + ": $" + priceRange.lowPrice + "-$" + priceRange.highPrice;

                    // Create font and brush.
                    Font drawFont = new Font("Arial", 10, FontStyle.Bold);
                    SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.White);
                    graphics.DrawString(fullCaption, drawFont, drawBrush, 0, 0);

                    drawnBitmap.Save(@"images\imageWithCaption" + i + ".jpg");
                
                }
                
            }



            // FileStream imageWriterStream = new FileStream(@"images\image0.jpg", FileMode.Open, FileAccess.Read);
            // Image image = Image.FromStream(imageWriterStream);
            // imageWriterStream.Close();

            // // //Bitmap b = new Bitmap(image);
            // // Graphics graphics = Graphics.FromImage(image);

            // // // Create font and brush.
            // // Font drawFont = new Font("Arial", 16);
            // // SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
            // // graphics.DrawString("Hello", drawFont, drawBrush, 0, 0);

            // image.Save(@"images\image0.jpg", ImageFormat.Jpeg);

            // image.Dispose();
            // //b.Dispose();

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
                "Scarabs",
                "Vessels",
                "Ceramics",
                "Wood"
            };

            Random rnd = new Random();
            string material = materials[ rnd.Next(0, materials.Length)];

            Console.WriteLine("Material: " + material);

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

        static bool DownloadImage(string url, string outputLocation)
        {
            try 
            {
                HttpWebRequest lxRequest = (HttpWebRequest)WebRequest.Create(url);

                // returned values are returned as a stream, then read into a string
                using (HttpWebResponse lxResponse = (HttpWebResponse)lxRequest.GetResponse()){
                    using (BinaryReader reader = new BinaryReader(lxResponse.GetResponseStream())) {
                        Byte[] lnByte = reader.ReadBytes(1 * 1024 * 1024 * 10);
                        using (FileStream lxFS = new FileStream(outputLocation, FileMode.OpenOrCreate)) {
                            lxFS.Write(lnByte, 0, lnByte.Length);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("exception thrown during get for " + url);
                return false;
            }
        }

        static async Task<AnalysisResult> AnalyzeImage(string filepath)
        {
                VisionServiceClient VisionServiceClient = new VisionServiceClient(computerVisionKey, "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");
            Console.WriteLine("VisionServiceClient is created");

            using (Stream imageFileStream = File.OpenRead(filepath))
            {
                //
                // Analyze the image for all visual features
                //
                Console.WriteLine("Calling VisionServiceClient.AnalyzeImageAsync()...");
            VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };
                AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(imageFileStream, visualFeatures);
                return analysisResult;
            }
        }

        static PriceRange GetPriceRange(string caption, Image image, double confidence)
        {
            PriceRange priceRange;
            priceRange.lowPrice = 0;
            priceRange.highPrice = 0;

            foreach ( char c in caption )
            {
                priceRange.highPrice += c;
            }

            priceRange.highPrice += image.Width + image.Height;

            priceRange.lowPrice = (int)(priceRange.highPrice * confidence);

            return priceRange;
        }
    }
}
