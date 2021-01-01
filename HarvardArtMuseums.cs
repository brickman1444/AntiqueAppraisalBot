using System.Collections.Generic;
using System.Linq;

namespace AppraisalBot
{
    public class HarvardArtMuseum : IArtSource
    {
        class SearchResult
        {
            public class Info
            {
                public int pages = 0;
            }

            public class Record
            {
                public string url = "";
                public string primaryimageurl = "";
                public int imagepermissionlevel = 0;
            }

            public Info info = new Info();
            public Record[] records = new Record[] { };
        }

        private readonly string apiKey = "";

        public HarvardArtMuseum()
        {
            apiKey = GetAPIKey();
        }

        static string GetAPIKey()
        {
            string enivornmentVariable = System.Environment.GetEnvironmentVariable("harvardArtMuseumsKey");

            if (!string.IsNullOrEmpty(enivornmentVariable))
            {
                return enivornmentVariable;
            }

            return System.IO.File.ReadAllText("localconfig/harvardArtMuseum.txt");
        }

        static string GetRandomClassification()
        {
            // TODO: Once https://github.com/bnolan001/RandomSelection/pull/1 is merged in, this should
            // be replaced with the old weighted random selection.
            string[] classifications = new string[] {
                "Armor",
                "Recreational+Artifacts",
                "Amulets",
                "Timepieces",
                "Furniture",
                "Mirrors",
                "Weapons+and+Ammunition",
                "Boxes",
                "Lighting+Devices",
                "Jewelry",
                "Tools+and+Equipment",
                "Ritual+Implements",
                "Textile+Arts",
                "Sculpture",
                "Vessels",
            };

            return classifications.RandomElement(new System.Random());
        }

        static string GetAPIURL(string apiKey, string classification, int page = 0)
        {
            string pageParameter = (page > 0 ? "&page=" + page.ToString() : "");

            return "https://api.harvardartmuseums.org/object?apikey=" + apiKey + "&hasimage=1&classification=" + classification + pageParameter;
        }

        public Art.Object GetRandomObject(System.Random random)
        {
            return HarvardArtMuseum.GetRandomObject(apiKey, random);
        }

        static Art.Object GetRandomObject(string apiKey, System.Random random)
        {
            string classification = GetRandomClassification();

            System.Console.WriteLine("Classification: " + classification);

            SearchResult pageCountResult = Web.GetWebResponse<SearchResult>(GetAPIURL(apiKey, classification));

            int randomPage = random.Next(pageCountResult.info.pages) + 1;

            System.Console.WriteLine(string.Format("Choosing page {0} out of {1}", randomPage, pageCountResult.info.pages ) );

            SearchResult randomPageResult = Web.GetWebResponse<SearchResult>(GetAPIURL(apiKey, classification, randomPage));

            SearchResult.Record randomRecord = randomPageResult.records.RandomElement(random);

            return new Art.Object { imageURL = randomRecord.primaryimageurl, listingURL = randomRecord.url, artSourceHashTag = "#harvardartmuseums" };
        }

        enum ImagePermissionLevel
        {
            DisplayAtAnySize = 0,
            DisplayAtMaximumPixelDimensionOf256px = 1,
            DoNotDisplayAnyImages = 2,
        }

        static Art.Object MakeArtObject(SearchResult.Record record)
        {
            if (string.IsNullOrEmpty(record.primaryimageurl)
            || string.IsNullOrEmpty(record.url)
            || record.imagepermissionlevel != (int)ImagePermissionLevel.DisplayAtAnySize)
            {
                return null;
            }

            return new Art.Object { imageURL = record.primaryimageurl, listingURL = record.url, artSourceHashTag = "#harvardartmuseums" };
        }
    }
}