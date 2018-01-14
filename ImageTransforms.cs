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
        }
    }

}