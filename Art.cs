using System.Collections.Generic;

namespace AppraisalBot
{
    public static class Art
    {
        public class Object
        {
            public string imageURL;
            public string listingURL;
            public string artSourceHashTag;
        }

        public static IEnumerable<Object> GetRandomObjects(int numObjects)
        {
            IArtSource artSource = new HarvardArtMuseum();

            return artSource.GetRandomObjects(numObjects);
        }
    }
}