using System.Collections.Generic;

namespace AppraisalBot
{
    public class Art
    {
        public class Object
        {
            public string imageURL;
            public string listingURL;
            public string artSourceHashTag;
        }

        private readonly IEnumerable<IArtSource> sources;

        public Art(IEnumerable<IArtSource> inSources)
        {
            sources = inSources;
        }

        public IEnumerable<Object> GetRandomObjects(int numObjects)
        {
            System.Random random = new System.Random();

            IArtSource artSource = sources.RandomElement(random);

            List<Art.Object> objects = new List<Art.Object>();
            for (int numberOfTries = 0; objects.Count < numObjects && numberOfTries < ( numObjects + 10 ); numberOfTries++) {
                Art.Object newObject = artSource.GetRandomObject(random);
                if ( newObject != null )
                {
                    objects.Add(newObject);
                }
            }

            return objects;
        }
    }
}