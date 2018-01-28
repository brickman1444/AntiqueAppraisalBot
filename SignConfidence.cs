using System;
using System.Linq;

using Microsoft.ProjectOxford.Vision.Contract;

namespace AppraisalBot
{
    static class SignDetection
    {
        public static bool IsSign(AnalysisBlob analysisResult)
        {
            return GetSignConfidence(analysisResult) >= 0.5f;
        }

        public static float GetSignConfidence(AnalysisBlob analysisResult)
        {
            float cumulativeConfidence = 0.0f;

            // if an image has any of these tags it's more likely to be a sign
            string[] signTags = {
                "text",
                "sign",
                "book",
            };

            foreach ( string tag in signTags )
            {
                if ( analysisResult.generalAnalysisResult.Description.Tags.Contains( tag ) )
                {
                    cumulativeConfidence += 0.5f;
                }
            }

            string[] signDescriptions = {
                "text",
                "sign",
                "book",
                "piece of paper",
            };

            foreach ( string description in signDescriptions )
            {
                if ( analysisResult.generalAnalysisResult.Description.Captions[0].Text.Contains( description ) )
                {
                    cumulativeConfidence += 0.5f;
                }
            }

            Console.WriteLine("Sign confidence: " + cumulativeConfidence);

            return cumulativeConfidence;
        }

    }

}