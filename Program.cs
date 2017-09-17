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
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://metmuseum.org/api/collection/collectionlisting?offset=0&pageSize=0&perPage=100&sortBy=Relevance&sortOrder=asc");

            HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();

            Stream receiveStream = response.GetResponseStream();
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            // Pipes the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader(receiveStream, encode);
            Console.WriteLine("\r\nResponse stream received.");
            Char[] read = new Char[256];
            // Reads 256 characters at a time.    
            int count = readStream.Read(read, 0, 256);
            Console.WriteLine("HTML...\r\n");
            while (count > 0)
            {
                // Dumps the 256 characters on a string and displays the string to the console.
                String str = new String(read, 0, count);
                Console.Write(str);
                count = readStream.Read(read, 0, 256);
            }
            Console.WriteLine("");
            // Releases the resources of the response.
            response.Close();
            // Releases the resources of the Stream.
            readStream.Close();
        }
    }
}
