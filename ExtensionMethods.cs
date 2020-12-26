using System.Collections.Generic;
using System.Linq;

public static class ExtensionMethods
{
    public static bool IsClipArt( this Microsoft.ProjectOxford.Vision.Contract.AnalysisResult analysisResult )
    {
        // ClipartType 
        // -Non-clipart = 0,
        // -ambiguous = 1,
        // -normal-clipart = 2,
        // -good-clipart = 3

        return analysisResult.ImageType.ClipArtType != 0;
    }

    public static bool IsLineDrawing( this Microsoft.ProjectOxford.Vision.Contract.AnalysisResult analysisResult )
    {
        // LineDrawingType
        // -Non-LineDrawing = 0,
        // -LineDrawing = 1.

        return analysisResult.ImageType.LineDrawingType != 0;
    }

    public static bool IsBlackAndWhite( this Microsoft.ProjectOxford.Vision.Contract.AnalysisResult analysisResult )
    {
        return analysisResult.Color.IsBWImg;
    }

    public static IEnumerable<T> RandomSubset<T>(this IEnumerable<T> originalList, int numberOfItemsToTake, System.Random rnd)
    {
        if (numberOfItemsToTake > originalList.Count())
        {
            throw new System.Exception("Not enough items to take subset. Requested: " + numberOfItemsToTake + " Have: " + originalList.Count());
        }

        return originalList.OrderBy(x => rnd.NextDouble()).Take(numberOfItemsToTake);
    }
}