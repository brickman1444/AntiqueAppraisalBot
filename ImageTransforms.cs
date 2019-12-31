using System;
using System.Numerics;

using SixLabors.ImageSharp.Processing;
using MathNet.Numerics.LinearAlgebra;
using SixLabors.Primitives;

using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    static class ImageTransforms
    {
        public static Bitmap ComposeImageOntoPhoto(Bitmap sourceArtImage)
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

            if (isWide)
            {
                r0prime = new Vector2(217, 168);
                r1prime = new Vector2(784, 161);
                r2prime = new Vector2(199, 597);
                r3prime = new Vector2(803, 593);
                backgroundImageName = "widePaperSource.jpg";
            }
            else
            {
                r0prime = new Vector2(187, 318);
                r1prime = new Vector2(553, 314);
                r2prime = new Vector2(155, 838);
                r3prime = new Vector2(584, 837);
                backgroundImageName = "tallPaperSource.jpg";
            }

            float leftSideHeight = Vector2.Distance(r0prime, r2prime);
            float rightSideHeight = Vector2.Distance(r1prime, r3prime);

            float bottomSideWidth = Vector2.Distance(r2prime, r3prime);

            // Keeping the source image's aspect ratio, what would it's height be in the destination
            float scaledHeight = (float)sourceArtImage.Height / (float)sourceArtImage.Width * bottomSideWidth;

            if (scaledHeight > leftSideHeight)
            {
                // I know this math isn't right. It slightly distorts the output but it gets the job done and
                // I can understand what it's doing.
                // TODO: Reduce the usage of this code by adding more background images at different aspect ratios
                r0prime = Vector2.Normalize(r0prime - r2prime) * scaledHeight + r2prime;
                r1prime = Vector2.Normalize(r1prime - r3prime) * scaledHeight * rightSideHeight / leftSideHeight + r3prime;
            }

            Bitmap photoImage = Program.LoadImage(Program.LoadImageType.Source, backgroundImageName);

            return PerspectiveTransform(sourceArtImage, photoImage, new Point((int)r0prime.X, (int)r0prime.Y), new Point((int)r1prime.X, (int)r1prime.Y), new Point((int)r2prime.X, (int)r2prime.Y), new Point((int)r3prime.X, (int)r3prime.Y));
        }

        public static Bitmap PerspectiveTransform(Bitmap sourceArtImage, Bitmap destinationImage, Point r0prime, Point r1prime, Point r2prime, Point r3prime)
        {
            // Corner numbering chosen arbitrarily
            // 0    1
            //
            // 2    3

            Matrix4x4 newMatrix = CalculateProjectiveTransformationMatrix(
                sourceArtImage.Width,
                sourceArtImage.Height,
                r0prime,
                r1prime,
                r2prime,
                r3prime);

            sourceArtImage.Mutate(x => x.Transform(new ProjectiveTransformBuilder().AppendMatrix(newMatrix)));

            destinationImage.Mutate(x => x.DrawImage(sourceArtImage, 1.0f));

            return destinationImage;
        }

        private static Matrix4x4 CalculateProjectiveTransformationMatrix(int width, int height, Point newTopLeft, Point newTopRight, Point newBottomLeft, Point newBottomRight)
        {
            Matrix<double> s = MapBasisToPoints(
                new Point(0, 0),
                new Point(width, 0),
                new Point(0, height),
                new Point(width, height)
            );
            Matrix<double> d = MapBasisToPoints(newTopLeft, newTopRight, newBottomLeft, newBottomRight);
            Matrix<double> result = d.Multiply(AdjugateMatrix(s));
            Matrix<double> normalized = result.Divide(result[2, 2]);
            return new Matrix4x4(
                (float)normalized[0, 0], (float)normalized[1, 0], 0, (float)normalized[2, 0],
                (float)normalized[0, 1], (float)normalized[1, 1], 0, (float)normalized[2, 1],
                0, 0, 1, 0,
                (float)normalized[0, 2], (float)normalized[1, 2], 0, (float)normalized[2, 2]
            );
        }
        private static Matrix<double> AdjugateMatrix(Matrix<double> matrix)
        {
            if (matrix.RowCount != 3 || matrix.ColumnCount != 3)
            {
                throw new ArgumentException("Must provide a 3x3 matrix.");
            }

            var adj = matrix.Clone();
            adj[0, 0] = matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1];
            adj[0, 1] = matrix[0, 2] * matrix[2, 1] - matrix[0, 1] * matrix[2, 2];
            adj[0, 2] = matrix[0, 1] * matrix[1, 2] - matrix[0, 2] * matrix[1, 1];
            adj[1, 0] = matrix[1, 2] * matrix[2, 0] - matrix[1, 0] * matrix[2, 2];
            adj[1, 1] = matrix[0, 0] * matrix[2, 2] - matrix[0, 2] * matrix[2, 0];
            adj[1, 2] = matrix[0, 2] * matrix[1, 0] - matrix[0, 0] * matrix[1, 2];
            adj[2, 0] = matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0];
            adj[2, 1] = matrix[0, 1] * matrix[2, 0] - matrix[0, 0] * matrix[2, 1];
            adj[2, 2] = matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

            return adj;
        }

        private static Matrix<double> MapBasisToPoints(Point p1, Point p2, Point p3, Point p4)
        {
            var A = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {p1.X, p2.X, p3.X},
                {p1.Y, p2.Y, p3.Y},
                {1, 1, 1}
            });
            var b = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(new double[] { p4.X, p4.Y, 1 });
            var aj = AdjugateMatrix(A);
            var v = aj.Multiply(b);
            var m = Matrix<double>.Build.DenseOfArray(new[,]
            {
                {v[0], 0, 0 },
                {0, v[1], 0 },
                {0, 0, v[2] }
            });
            return A.Multiply(m);
        }
    }

}