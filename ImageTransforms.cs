using System;
using System.Numerics;

using SixLabors.ImageSharp.Processing;
using MathNet.Numerics.LinearAlgebra;

using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace AppraisalBot
{
    static class ImageTransforms
    {
        public static Bitmap ComposeImageOntoPhoto(Bitmap sourceArtImage)
        {
            bool isWide = sourceArtImage.Width > sourceArtImage.Height;

            // These will be transformed corners of the image in the destination space.
            Vector2 newTopLeft = new Vector2();
            Vector2 newTopRight = new Vector2();
            Vector2 newBottomLeft = new Vector2();
            Vector2 newBottomRight = new Vector2();
            string backgroundImageName = "";

            // Find which image fits the source best. The vector values are taken from
            // where the reference object's corners in the source image are. After
            // composing the image, the art image will cover up the reference object.

            // TODO: Turn this into a data file instead of code.
            // TODO: Outside this function, decide whether it is a painting or paper and choose relevant background image.

            if (isWide)
            {
                newTopLeft = new Vector2(217, 168);
                newTopRight = new Vector2(784, 161);
                newBottomLeft = new Vector2(199, 597);
                newBottomRight = new Vector2(803, 593);
                backgroundImageName = "widePaperSource.jpg";
            }
            else
            {
                newTopLeft = new Vector2(187, 318);
                newTopRight = new Vector2(553, 314);
                newBottomLeft = new Vector2(155, 838);
                newBottomRight = new Vector2(584, 837);
                backgroundImageName = "tallPaperSource.jpg";
            }

            float leftSideHeight = Vector2.Distance(newTopLeft, newBottomLeft);
            float rightSideHeight = Vector2.Distance(newTopRight, newBottomRight);

            float bottomSideWidth = Vector2.Distance(newBottomLeft, newBottomRight);

            // Keeping the source image's aspect ratio, what would it's height be in the destination
            float scaledHeight = (float)sourceArtImage.Height / (float)sourceArtImage.Width * bottomSideWidth;

            if (scaledHeight > leftSideHeight)
            {
                // I know this math isn't right. It slightly distorts the output but it gets the job done and
                // I can understand what it's doing.
                // TODO: Reduce the usage of this code by adding more background images at different aspect ratios
                newTopLeft = Vector2.Normalize(newTopLeft - newBottomLeft) * scaledHeight + newBottomLeft;
                newTopRight = Vector2.Normalize(newTopRight - newBottomRight) * scaledHeight * rightSideHeight / leftSideHeight + newBottomRight;
            }

            Bitmap photoImage = Program.LoadImage(Program.LoadImageType.Source, backgroundImageName);

            return PerspectiveTransform(sourceArtImage, photoImage, newTopLeft, newTopRight, newBottomLeft, newBottomRight);
        }

        public static Bitmap PerspectiveTransform(Bitmap sourceArtImage, Bitmap destinationImage, Vector2 newTopLeft, Vector2 newTopRight, Vector2 newBottomLeft, Vector2 newBottomRight)
        {
            // Corner numbering chosen arbitrarily
            // 0    1
            //
            // 2    3

            Matrix4x4 newMatrix = CalculateProjectiveTransformationMatrix(
                sourceArtImage.Width,
                sourceArtImage.Height,
                newTopLeft,
                newTopRight,
                newBottomLeft,
                newBottomRight);

            sourceArtImage.Mutate(x => x.Transform(new ProjectiveTransformBuilder().AppendMatrix(newMatrix)));

            destinationImage.Mutate(x => x.DrawImage(sourceArtImage, 1.0f));

            return destinationImage;
        }

        private static Matrix4x4 CalculateProjectiveTransformationMatrix(int width, int height, Vector2 newTopLeft, Vector2 newTopRight, Vector2 newBottomLeft, Vector2 newBottomRight)
        {
            Matrix<double> s = MapBasisToPoints(
                new Vector2(0, 0),
                new Vector2(width, 0),
                new Vector2(0, height),
                new Vector2(width, height)
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

        private static Matrix<double> MapBasisToPoints(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
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