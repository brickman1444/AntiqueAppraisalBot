using System;
using System.Net;
using System.IO;
using System.Text;

namespace AppraisalBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://metmuseum.org/api/collection/collectionlisting?offset=0&pageSize=0&perPage=100&sortBy=Relevance&sortOrder=asc");

            HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();

            Stream receiveStream = response.GetResponseStream();
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            // Pipes the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader(receiveStream, encode);
            Console.WriteLine("\r\nResponse stream received.");

            string line;
            while ((line = readStream.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }

            Console.WriteLine("");
            // Releases the resources of the response.
            response.Close();
            // Releases the resources of the Stream.
            readStream.Close();
        }
    }
}
