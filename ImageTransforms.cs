using System.IO;
using System.Numerics;

using SixLabors.ImageSharp;
using SixLabors.Primitives;

using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.Rgba32>;

namespace AppraisalBot
{
    static class ImageTransforms
    {
        public static void PerspectiveTransform( Bitmap sourceImage )
        {
            using ( Bitmap transformedImage = sourceImage.Clone( ctx => ctx.Resize( 10, 10 ) ) )
            {
                if ( Directory.Exists("images"))
                {
                    string destinationFilePath = @"images/transformed.jpg";
                    transformedImage.Save( destinationFilePath );
                }
            }

            PointF r0 = new PointF( 0,                 0 );
            PointF r1 = new PointF( sourceImage.Width, 0 );
            PointF r2 = new PointF( 0,                 sourceImage.Height );
            PointF r3 = new PointF( sourceImage.Width, sourceImage.Height );

            PointF r0prime = new PointF( 125, 50 );
            PointF r1prime = new PointF( 200, 100 );
            PointF r2prime = new PointF( 50, 125 );
            PointF r3prime = new PointF( 225, 200 );

            // Reference for where these numbers come from:
            // http://www.vis.uky.edu/~ryang/Teaching/cs635-2016spring/Lectures/05-geo_trans_1.pdf
            double[,] systemOfEquations = {
                {r0.X, r0.Y, 1.0, 0.0, 0.0, 0.0, -r0.X * r0prime.X, -r0.Y * r0prime.X},
                {r1.X, r1.Y, 1.0, 0.0, 0.0, 0.0, -r1.X * r1prime.X, -r1.Y * r1prime.X},
                {r2.X, r2.Y, 1.0, 0.0, 0.0, 0.0, -r2.X * r2prime.X, -r2.Y * r2prime.X},
                {r3.X, r3.Y, 1.0, 0.0, 0.0, 0.0, -r3.X * r3prime.X, -r3.Y * r3prime.X},

                {0.0, 0.0, 0.0, r0.X, r0.Y, 1.0, -r0.X * r0prime.Y, -r0.Y * r0prime.Y},
                {0.0, 0.0, 0.0, r1.X, r1.Y, 1.0, -r1.X * r1prime.Y, -r1.Y * r1prime.Y},
                {0.0, 0.0, 0.0, r2.X, r2.Y, 1.0, -r2.X * r2prime.Y, -r2.Y * r2prime.Y},
                {0.0, 0.0, 0.0, r3.X, r3.Y, 1.0, -r3.X * r3prime.Y, -r3.Y * r3prime.Y},
            };

            double[] otherSideOfTheEqualsSign = { 
                r0prime.X,
                r1prime.X,
                r2prime.X,
                r3prime.X,
                r0prime.Y,
                r1prime.Y,
                r2prime.Y,
                r3prime.Y,
            };

            double[] solveResults = StarMathLib.StarMath.solve( systemOfEquations, otherSideOfTheEqualsSign );

            Matrix4x4 perspectiveTransform = new Matrix4x4(
                (float)solveResults[0], (float)solveResults[3], (float)solveResults[6], 0.0f,
                (float)solveResults[1], (float)solveResults[4], (float)solveResults[7], 0.0f,
                (float)solveResults[2], (float)solveResults[5], 1.0f,                   0.0f,
                0.0f,                   0.0f,                   0.0f,                   0.0f);

            Vector4 testPoint2 = System.Numerics.Vector4.Transform( new Vector4( r0.X, r0.Y, 1.0f, 0.0f ), perspectiveTransform );
            Vector4 normalizedTestPoint2 = testPoint2 / testPoint2.Z;
        }
    }

}