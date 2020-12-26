using System.Collections.Generic;

namespace AppraisalBot
{
    public static class Art
    {
        public class Object
        {
            public string imageURL;
            public string listingURL;
        }

        public static IEnumerable<Object> GetRandomObjects(int numObjects)
        {
            IArtSource artSource = new MetropolitanMuseumOfArt();

            return artSource.GetRandomObjects(numObjects);
        }
    }
}