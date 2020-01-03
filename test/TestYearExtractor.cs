using System.Linq;

using Xunit;

namespace AppraisalBot
{
    public static class TestYearExtractor
    {
        [Theory]
        [InlineData(1900, 1900)]
        public static void TestPriority(int year, int expectedPriority)
        {
            Assert.Equal(YearExtractor.GetPriority(year), expectedPriority);
        }

        public static System.Collections.Generic.IEnumerable<object[]> GetYearExtractTestData()
        {
            yield return new object[] { new System.Collections.Generic.List<string> { "1900" }, 1900 };
            yield return new object[] { new System.Collections.Generic.List<string> {}, null };
            yield return new object[] { new System.Collections.Generic.List<string> { "a", "b", "c" }, null };
            yield return new object[] { null, null };
            yield return new object[] { new System.Collections.Generic.List<string> { "1900", "404", "2000" }, 2000 };
            yield return new object[] { new System.Collections.Generic.List<string> { "10", "1", "2" }, null };
            yield return new object[] { new System.Collections.Generic.List<string> { "a", "404" }, 404 };
        }

        [Theory]
        [MemberData(nameof(GetYearExtractTestData))]
        public static void BestYearExtractedFromList(System.Collections.Generic.IEnumerable<string> words, int? expectedYear)
        {
            int? actualYear = YearExtractor.ExtractYear(words);

            Assert.Equal(expectedYear, actualYear);
        }
    }
}