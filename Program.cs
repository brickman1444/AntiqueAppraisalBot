using System;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

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
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string responseText = GetCollectionListing()
            MetResponse responseObject = JsonConvert.DeserializeObject<MetResponse>(responseText);

            DownloadImage();
        }

        static string GetCollectionListing()
        {
            string url = "http://metmuseum.org/api/collection/collectionlisting?offset=0&pageSize=0&perPage=100&sortBy=Relevance&sortOrder=asc";

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

        static void DownloadImage()
        {
            HttpWebRequest lxRequest = (HttpWebRequest)WebRequest.Create(
"http://www.productimageswebsite.com/images/stock_jpgs/34891.jpg");

            // returned values are returned as a stream, then read into a string
            using (HttpWebResponse lxResponse = (HttpWebResponse)lxRequest.GetResponse()){
                using (BinaryReader reader = new BinaryReader(lxResponse.GetResponseStream())) {
                    Byte[] lnByte = reader.ReadBytes(1 * 1024 * 1024 * 10);
                    using (FileStream lxFS = new FileStream("image.jpg", FileMode.Create)) {
                        lxFS.Write(lnByte, 0, lnByte.Length);
                    }
                }
            }
        }
    }
}
