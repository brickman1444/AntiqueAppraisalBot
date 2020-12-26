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
            }

            public Info info = new Info();
            public Record[] records = new Record[] { };
        }

        public IEnumerable<Art.Object> GetRandomObjects(int numItems)
        {
            string apiKey = GetAPIKey();

            System.Random random = new System.Random();

            return new Art.Object[] { GetRandomObject(apiKey, random) };
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
            BNolan.RandomSelection.Selector<string> classificationSelector = new BNolan.RandomSelection.Selector<string>();

            // Weights come from how many items fit the search criteria. This can be found by hitting:
            // https://api.harvardartmuseums.org/object?apikey=API_KEY&hasimage=1&classification=Furniture
            // and replacing Furniture with the name of the material.
            // For scale, there are around 200,000 total objects.
            classificationSelector.TryAddItem("Armor", "Armor", 50); // 37
            classificationSelector.TryAddItem("Recreational+Artifacts", "Recreational+Artifacts", 50); // 72
            classificationSelector.TryAddItem("Amulets", "Amulets", 100); // 138
            classificationSelector.TryAddItem("Timepieces", "Timepieces", 150); // 139
            classificationSelector.TryAddItem("Furniture", "Furniture", 150); // 141
            classificationSelector.TryAddItem("Mirrors", "Mirrors", 100); // 142
            classificationSelector.TryAddItem("Weapons+and+Ammunition", "Weapons+and+Ammunition", 200); // 184
            classificationSelector.TryAddItem("Boxes", "Boxes", 150); // 223
            classificationSelector.TryAddItem("Lighting+Devices", "Lighting+Devices", 300); // 457
            classificationSelector.TryAddItem("Jewelry", "Jewelry", 200); // 680
            classificationSelector.TryAddItem("Tools+and+Equipment", "Tools+and+Equipment", 300); // 637
            classificationSelector.TryAddItem("Ritual+Implements", "Ritual+Implements", 200); // 902
            classificationSelector.TryAddItem("Textile+Arts", "Textile+Arts", 200); // 1833
            classificationSelector.TryAddItem("Sculpture", "Sculpture", 300); // 4549
            classificationSelector.TryAddItem("Vessels", "Vessels", 150); // 5021

            return classificationSelector.RandomSelect(1).First().Value;
        }

        static string GetAPIURL(string apiKey, string classification, int page = 0)
        {
            string pageParameter = (page > 0 ? "&page=" + page.ToString() : "");

            return "https://api.harvardartmuseums.org/object?apikey=" + apiKey + "&hasimage=1&classification=" + classification + pageParameter;
        }

        static Art.Object GetRandomObject(string apiKey, System.Random random)
        {
            string classification = GetRandomClassification();

            System.Console.WriteLine("Classification: " + classification);

            SearchResult pageCountResult = Web.GetWebResponse<SearchResult>(GetAPIURL(apiKey, classification));

            int randomPage = random.Next(pageCountResult.info.pages) + 1;

            SearchResult randomPageResult = Web.GetWebResponse<SearchResult>(GetAPIURL(apiKey, classification, randomPage));

            int randomRecordIndex = random.Next(randomPageResult.records.Count());

            SearchResult.Record randomRecord = randomPageResult.records[randomRecordIndex];

            return new Art.Object { imageURL = randomRecord.primaryimageurl, listingURL = randomRecord.url };
        }
    }
}