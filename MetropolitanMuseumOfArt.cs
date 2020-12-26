using System.Collections.Generic;
using System.Linq;

namespace AppraisalBot
{
    public class MetropolitanMuseumOfArt : IArtSource
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

        public IEnumerable<Art.Object> GetRandomObjects(int numItems)
        {
            System.Random random = new System.Random();
            IEnumerable<int> randomSetOfObjectIDs = GetRandomObjectIDs(numItems, random);

            return randomSetOfObjectIDs
            .Select(objectID => GetObjectResponse(objectID))
            .Select(metResponse => new Art.Object{imageURL = metResponse.primaryImage, listingURL = metResponse.objectURL});
        }

        static IEnumerable<int> GetRandomObjectIDs(int numItems, System.Random random)
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

            MetSearchResponse response = Web.GetWebResponse<MetSearchResponse>(searchURL);

            System.Console.WriteLine("Total items in category: " + response.total);

            System.Console.WriteLine("Material: " + material + " numItems: " + numItems);

            if (numItems > response.total)
            {
                throw new System.Exception("Not enough items meet search criteria. Requested: " + numItems + " Found: " + response.total);
            }

            return response.objectIDs.RandomSubset(numItems, random);
        }

        static string GetMetSearchAPIUrl(string material)
        {
            return "https://collectionapi.metmuseum.org/public/collection/v1/search"
            + "?medium=" + material
            + "&q=" + material // The endpoint doesn't return anything if q is not supplied.
            + "&hasImages=true";
        }

        static MetObjectResponse GetObjectResponse(int objectID)
        {
            return Web.GetWebResponse<MetObjectResponse>(GetMetObjectAPIUrl(objectID));
        }
        
        static string GetMetObjectAPIUrl(int objectID)
        {
            return "https://collectionapi.metmuseum.org/public/collection/v1/objects/" + objectID;
        }
    }
}