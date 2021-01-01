
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace AppraisalBot
{
    public class MockArtSource : IArtSource
    {
        private readonly IEnumerator<Art.Object> objectsEnumerator;

        public MockArtSource(IEnumerable<Art.Object> inObjects)
        {
            objectsEnumerator = inObjects.GetEnumerator();
        }

        public Art.Object GetRandomObject(System.Random random)
        {
            bool hasNextObject = objectsEnumerator.MoveNext();
            if ( hasNextObject )
            {
                return objectsEnumerator.Current;
            }
            else
            {
                return null;
            }
        }
    }

    public static class TestArt
    {
        [Fact]
        public static void SingleObjectIsReturnedWhenItsAvailable()
        {
            MockArtSource singleObjectSource = new MockArtSource(new Art.Object[] { new Art.Object() });

            Art art = new Art(new IArtSource[] { singleObjectSource });

            IEnumerable<Art.Object> objects = art.GetRandomObjects(1);

            Assert.Single(objects);
            Assert.NotNull(objects.First());
        }

        [Fact]
        public static void WhenArtSourceAlwaysFailsNoObjectsAreReturned()
        {
            MockArtSource alwaysFailArtSource = new MockArtSource(new Art.Object[] { });

            Art art = new Art(new IArtSource[] { alwaysFailArtSource });

            IEnumerable<Art.Object> objects = art.GetRandomObjects(1);

            Assert.Empty(objects);
        }

        [Fact]
        public static void WhenArtSourceSometimesFailsTheRequestedNumberOfObjectsIsReturns()
        {
            Art.Object[] sourceObjects = new Art.Object[]{ null, new Art.Object(), null, new Art.Object(), null, new Art.Object() };

            MockArtSource sometimesFailSource = new MockArtSource(sourceObjects);

            Art art = new Art(new IArtSource[] { sometimesFailSource });

            IEnumerable<Art.Object> objects = art.GetRandomObjects(1);

            Assert.Single(objects);
            Assert.NotNull(objects.First());
        }
    }
}