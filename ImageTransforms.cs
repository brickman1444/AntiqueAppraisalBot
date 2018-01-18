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
            PointF r0 = new PointF( 0,                 0 );
            PointF r1 = new PointF( sourceImage.Width, 0 );
            PointF r2 = new PointF( 0,                 sourceImage.Height );
            PointF r3 = new PointF( sourceImage.Width, sourceImage.Height );

            PointF r0prime = new PointF( 125, sourceImage.Height / 2 );
            PointF r1prime = new PointF( 200, sourceImage.Height / 2 );
            PointF r2prime = new PointF( 0, (sourceImage.Height * 3) / 4 );
            PointF r3prime = new PointF( sourceImage.Width, sourceImage.Height );

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
                0.0f,                   0.0f,                   0.0f,                   1.0f);

            // Matrix4x4 invertedPerspectiveTransform = new Matrix4x4();
            // Matrix4x4.Invert( perspectiveTransform, out invertedPerspectiveTransform );

            // double[,] testTransform = {
            //    { solveResults[0], solveResults[3], solveResults[6], 0.0f, },
            //    { solveResults[1], solveResults[4], solveResults[7], 0.0f, },
            //    { solveResults[2], solveResults[5], 1.0f,            0.0f, },
            //    { 0.0f,            0.0f,            0.0f,            0.0f  } };

            // double[,] testTransformInvert = StarMathLib.StarMath.inverse( testTransform );

            Bitmap imageCopy = new Bitmap( sourceImage.Width, sourceImage.Height );

            foreach ( ImageFrame<Rgba32> sourceFrame in sourceImage.Frames )
            {
                for ( int sourceY = 0; sourceY < imageCopy.Height; sourceY++ )
                {
                    for ( int sourceX = 0; sourceX < imageCopy.Width; sourceX++ )
                    {
                        Vector4 sourcePoint = new Vector4( sourceX, sourceY, 1.0f, 0.0f );

                        Vector4 destinationPoint = Vector4.Transform( sourcePoint, perspectiveTransform );

                        destinationPoint /= destinationPoint.Z;

                        // This is where you'd want to sample differently if you're into that
                        if ( destinationPoint.X >= 0 && destinationPoint.Y >= 0
                        && destinationPoint.X < imageCopy.Width && destinationPoint.Y < imageCopy.Height )
                        {
                            imageCopy[ (int)destinationPoint.X, (int)destinationPoint.Y ] = sourceImage[ sourceX, sourceY ];
                        }
                    }
                }
            }

            if ( Directory.Exists("images"))
            {
                string destinationFilePath = @"images/transformed.jpg";
                imageCopy.Save( destinationFilePath );
            }
        }
    }

}