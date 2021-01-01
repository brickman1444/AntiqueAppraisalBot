
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace AppraisalBot
{
    public static class TestExtensionMethods
    {
        [Fact]
        public static void RandomElementSelectsAllElementsEventually()
        {
            int[] sourceNumbers = new int[]{ 1, 2, 3, 4, 5, 6 };

            HashSet<int> outputNumbers = new HashSet<int>();

            System.Random random = new System.Random();

            for ( int numberOfSelections = 0; numberOfSelections < 100 && outputNumbers.Count < sourceNumbers.Count(); numberOfSelections++ )
            {
                outputNumbers.Add(sourceNumbers.RandomElement(random));
            }

            foreach ( int sourceNumber in sourceNumbers )
            {
                Assert.Contains(sourceNumber, outputNumbers);
            }
        }
    }
}