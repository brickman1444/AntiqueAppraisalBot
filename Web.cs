using PixelColor = SixLabors.ImageSharp.PixelFormats.Rgba32;
using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    public static class Web
    {
        public static T GetWebResponse<T>(string url)
        {
            System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

            using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponseAsync().GetAwaiter().GetResult())
            {
                using (System.IO.StreamReader readStream = new System.IO.StreamReader(response.GetResponseStream(), encode))
                {
                    string responseText = readStream.ReadToEnd();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseText);
                }
            }
        }

        public static Bitmap DownloadImage(string url)
        {
            try
            {
                System.Net.HttpWebRequest lxRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

                // returned values are returned as a stream, then read into a string
                using (System.Net.HttpWebResponse lxResponse = (System.Net.HttpWebResponse)lxRequest.GetResponseAsync().GetAwaiter().GetResult())
                {

                    Bitmap image = SixLabors.ImageSharp.Image.Load<PixelColor>(lxResponse.GetResponseStream());

                    if (image.Width >= 250)
                    {
                        return image;
                    }
                    else
                    {
                        System.Console.WriteLine("throwing out image because it's too small. Width: " + image.Width);
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("exception thrown during get for " + url + " " + e);
            }
            return null;
        }
    }
}