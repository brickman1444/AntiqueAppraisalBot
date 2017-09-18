using System;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace AppraisalBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string responseText = RunWebRequest("http://metmuseum.org/api/collection/collectionlisting?offset=0&pageSize=0&perPage=100&sortBy=Relevance&sortOrder=asc");

            Console.WriteLine(responseText);
        }

        static string RunWebRequest(string url)
        {
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
    }
}
