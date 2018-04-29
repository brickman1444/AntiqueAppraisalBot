using System;
using System.IO;
using System.Numerics;

using SixLabors.ImageSharp;
using SixLabors.Primitives;

using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.Rgba32>;

namespace AppraisalBot
{
    static class ImageTransforms
    {
        public static Bitmap ComposeImageOntoPhoto( Bitmap sourceArtImage )
        {
            bool isWide = sourceArtImage.Width > sourceArtImage.Height;

            // These will be transformed corners of the image in the destination space.
            Vector2 r0prime = new Vector2();
            Vector2 r1prime = new Vector2();
            Vector2 r2prime = new Vector2();
            Vector2 r3prime = new Vector2();
            string backgroundImageName = "";

            // Find which image fits the source best. The vector values are taken from
            // where the reference object's corners in the source image are. After
            // composing the image, the art image will cover up the reference object.
            
            // TODO: Turn this into a data file instead of code.
            // TODO: Outside this function, decide whether it is a painting or paper and choose relevant background image.

            if ( isWide )
            {
                r0prime = new Vector2( 217, 168 );
                r1prime = new Vector2( 784, 161 );
                r2prime = new Vector2( 199, 597 );
                r3prime = new Vector2( 803, 593 );
                backgroundImageName = "widePaperSource.jpg";
            }
            else
            {
                r0prime = new Vector2( 187, 318 );
                r1prime = new Vector2( 553, 314 );
                r2prime = new Vector2( 155, 838 );
                r3prime = new Vector2( 584, 837 );
                backgroundImageName = "tallPaperSource.jpg";
            }

            float leftSideHeight = Vector2.Distance( r0prime, r2prime );
            float rightSideHeight = Vector2.Distance( r1prime, r3prime );

            float bottomSideWidth = Vector2.Distance( r2prime, r3prime );

            // Keeping the source image's aspect ratio, what would it's height be in the destination
            float scaledHeight = (float)sourceArtImage.Height / (float)sourceArtImage.Width * bottomSideWidth;

            if ( scaledHeight > leftSideHeight )
            {
                // I know this math isn't right. It slightly distorts the output but it gets the job done and
                // I can understand what it's doing.
                // TODO: Reduce the usage of this code by adding more background images at different aspect ratios
                r0prime = Vector2.Normalize(r0prime - r2prime) * scaledHeight + r2prime;
                r1prime = Vector2.Normalize(r1prime - r3prime) * scaledHeight * rightSideHeight / leftSideHeight + r3prime;
            }

            Bitmap photoImage = Image.Load( "sourceArt/" + backgroundImageName );

            return PerspectiveTransform( sourceArtImage, photoImage, r0prime, r1prime, r2prime, r3prime );
        }

        public static Bitmap PerspectiveTransform( Bitmap sourceArtImage, Bitmap destinationImage, Vector2 r0prime, Vector2 r1prime, Vector2 r2prime, Vector2 r3prime )
        {
            // Corner numbering chosen arbitrarily
            // 0    1
            //
            // 2    3
            
            // Locations of the art image corners in source space.
            Vector2 r0 = new Vector2( 0,                    0 );
            Vector2 r1 = new Vector2( sourceArtImage.Width, 0 );
            Vector2 r2 = new Vector2( 0,                    sourceArtImage.Height );
            Vector2 r3 = new Vector2( sourceArtImage.Width, sourceArtImage.Height );

            // To transform the image into destination space, we will need a perspective
            // matrix. We will solve a system of equations and then use the results from
            // that to create the perspective matrix. Reference for this math is at:
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

            // Perspective matrix transforms a point from source space to destination space.
            Matrix4x4 perspectiveTransform = new Matrix4x4(
                (float)solveResults[0], (float)solveResults[3], (float)solveResults[6], 0.0f,
                (float)solveResults[1], (float)solveResults[4], (float)solveResults[7], 0.0f,
                (float)solveResults[2], (float)solveResults[5], 1.0f,                   0.0f,
                0.0f,                   0.0f,                   0.0f,                   1.0f);

            // Inverted perspective matrix transforms a point from destination space to source space.
            Matrix4x4 invertedPerspectiveTransform = new Matrix4x4();
            Matrix4x4.Invert( perspectiveTransform, out invertedPerspectiveTransform );

            // This could probably be optimized to only iterate through the pixels that matter.
            for ( int destinationY = 0; destinationY < destinationImage.Height; destinationY++ )
            {
                for ( int destinationX = 0; destinationX < destinationImage.Width; destinationX++ )
                {
                    Vector4 destinationPoint = new Vector4( destinationX, destinationY, 1.0f, 0.0f );

                    Vector4 sourcePoint = Vector4.Transform( destinationPoint, invertedPerspectiveTransform );

                    sourcePoint /= sourcePoint.Z; // Normalize 2D homogenous coordinates

                    if ( sourcePoint.X >= 0 && sourcePoint.Y >= 0
                    && sourcePoint.X < sourceArtImage.Width && sourcePoint.Y < sourceArtImage.Height )
                    {
                        // This is where you'd want to sample differently if you're into that
                        destinationImage[ destinationX, destinationY ] = sourceArtImage[ (int)sourcePoint.X, (int)sourcePoint.Y ];
                    }
                }
            }

            return destinationImage;
        }
    }

}