using System;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;

using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

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

            int numItems = 1;

            Random rnd = new Random();
            int collectionOffset = rnd.Next(0,1950);

            string responseText = GetCollectionListing( numItems, collectionOffset );
            MetResponse responseObject = JsonConvert.DeserializeObject<MetResponse>(responseText);

            Console.WriteLine( "Found " + responseObject.results.Count + " results" );

            for ( int i = 0; i < responseObject.results.Count; i++ )
            {
                string fileLocation = "images/image" + i + ".jpg";
                DownloadImage(responseObject.results[i].image, fileLocation );
                AnalysisResult analysisResult = AnalyzeImage( fileLocation ).GetAwaiter().GetResult();

                foreach ( Caption caption in analysisResult.Description.Captions )
                {
                    Console.WriteLine( "Caption: " + caption.Text + " " + caption.Confidence );
                }
                
                foreach ( Category category in analysisResult.Categories )
                {
                    Console.WriteLine( "Category: " + category.Name + " " + category.Score);
                }
            }

            Console.WriteLine("Done");
        }

        static string GetCollectionListing(int numItems, int offset)
        {
            string url = "http://metmuseum.org/api/collection/collectionlisting?offset=" + offset + "&pageSize=0&perPage=" + numItems + "&sortBy=Relevance&sortOrder=asc";

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

        static void DownloadImage(string url, string outputLocation)
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
            }
            catch (Exception e)
            {
                Console.WriteLine("exception thrown during get for " + url);
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
    }
}
