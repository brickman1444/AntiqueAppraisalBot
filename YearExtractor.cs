using System.Linq;

namespace AppraisalBot
{
    public static class YearExtractor
    {
        public static int? ExtractYear(Microsoft.ProjectOxford.Vision.Contract.OcrResults textAnalysisResults)
        {
            System.Collections.Generic.List<string> words = new System.Collections.Generic.List<string>();

            foreach (Microsoft.ProjectOxford.Vision.Contract.Region region in textAnalysisResults.Regions)
            {
                foreach (Microsoft.ProjectOxford.Vision.Contract.Line line in region.Lines)
                {
                    foreach (Microsoft.ProjectOxford.Vision.Contract.Word word in line.Words)
                    {
                        words.Append(word.Text);
                    }
                }
            }

            return ExtractYear(words);
        }

        public static int? ExtractYear(System.Collections.Generic.IEnumerable<string> words)
        {
            if (words is null)
            {
                return null;
            }

            System.Collections.Generic.List<int> parsedYears = new System.Collections.Generic.List<int>();

            foreach (string word in words)
            {
                int year;
                bool successfullyParsed = int.TryParse(word, out year);

                if (successfullyParsed)
                {
                    parsedYears.Add(year);
                }
            }

            System.Collections.Generic.IEnumerable<int> yearsWithValidPriorities = parsedYears.Where(year => GetPriority(year) != 0);

            if (yearsWithValidPriorities.Count() == 0)
            {
                return null;
            }

            return yearsWithValidPriorities.OrderByDescending(year => GetPriority(year)).First();
        }

        public static int GetPriority(int year)
        {
            if (year > 2020 || year <= 100)
            {
                return 0;
            }

            return year;
        }
    }
}