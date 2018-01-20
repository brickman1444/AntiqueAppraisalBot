
using System.Linq;

using Microsoft.ProjectOxford.Vision.Contract;

namespace AppraisalBot
{
    static class PaintingDetection
    {
        public static bool IsPainting(AnalysisBlob analysisResult)
        {
            return GetPaintingConfidence(analysisResult) >= 0.5f;
        }

        public static float GetPaintingConfidence(AnalysisBlob analysisResult)
        {
            if ( analysisResult.generalAnalysisResult.IsClipArt()
            || analysisResult.generalAnalysisResult.IsLineDrawing() )
            {
                return 0.0f;
            }

            if ( analysisResult.celebrityAnalysisResult.celebrities.Count() > 0 )
            {
                return 1.0f;
            }

            if ( analysisResult.generalAnalysisResult.IsBlackAndWhite() )
            {
                return 0.0f;
            }

            float cumulativeConfidence = 0.0f;

            // if an image has any of these tags it's more likely to be a painting
            string[] paintingTags = {
                "view",
                "outdoor",
                "mountain",
                "grass",
                "posing",
                "photo",
                "painting",
                "man",
                "woman",
                "mirror",
                "people",
                "group",
            };

            foreach ( string tag in paintingTags )
            {
                if ( analysisResult.generalAnalysisResult.Description.Tags.Contains( tag ) )
                {
                    cumulativeConfidence += 0.1f;
                }
            }

            // if an image has any of these tags it's less likely to be a painting
            string[] notPaintingTags = {
                "stone",
                "wooden",
                "sign",
            };

            foreach ( string tag in notPaintingTags )
            {
                if ( analysisResult.generalAnalysisResult.Description.Tags.Contains( tag ) )
                {
                    cumulativeConfidence -= 0.1f;
                }
            }

            string[] paintingDescriptions = {
                "standing in a room",
                "posing for the camera",
                "looking at the camera",
                "a view of",
                "a close up of a person",
                "painting",
                "in front of a mirror",
                "a group of people",
            };

            foreach ( string description in paintingDescriptions )
            {
                if ( analysisResult.generalAnalysisResult.Description.Captions[0].Text.Contains( description ) )
                {
                    cumulativeConfidence += 0.4f;
                }
            }

            return cumulativeConfidence;
        }

    }

}