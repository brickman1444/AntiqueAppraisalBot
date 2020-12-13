using System;

using Rgba32 = SixLabors.ImageSharp.PixelFormats.Rgba32;
using Color = SixLabors.ImageSharp.Color;

namespace AppraisalBot
{
    static class ColorTable
    {
        public static string GetClosestColorName( string hexString )
        {
            return ColorTable.GetClosestColorName(ColorTable.GetColorFromHexString(hexString));
        }

        struct ColorData
        {
            
            public readonly Rgba32 color;
            public readonly string name;

            public ColorData( Rgba32 inColor, string inName )
            {
                color = inColor;
                name = inName;
            }
        }

        static ColorData[] table = {
            new ColorData( Color.Red, "red" ),
            new ColorData( Color.Blue, "blue" ),
            new ColorData( Color.Green, "green" ),

            new ColorData( Color.Cyan, "cyan" ),
            new ColorData( Color.Magenta, "magenta" ),
            new ColorData( Color.Yellow, "yellow" ),

            new ColorData( Color.Black, "black" ),
            new ColorData( Color.DimGray, "dark gray" ),
            new ColorData( Color.Gray, "gray" ),
            new ColorData( Color.LightGray, "light gray" ),
            new ColorData( Color.White, "white" ),

            new ColorData( Color.Orange, "orange" ),
            new ColorData( Color.SaddleBrown, "brown" ),
            new ColorData( Color.Sienna, "light brown" ),
            new ColorData( Color.Tan, "tan" ),
            new ColorData( Color.Purple, "purple" ),
            new ColorData( Color.Pink, "pink" ),
        };

        static Rgba32 GetColorFromHexString( string hexString )
        {
            Rgba32 outColor = new Rgba32();
            SixLabors.ImageSharp.PixelFormats.RgbaVector.FromHex( hexString ).ToRgba32(ref outColor);
            return outColor;
        }

        static string GetClosestColorName( Rgba32 color )
        {
            double closestSquaredDistance = SquaredColorDistance( color, table[0].color );
            ColorData closestColor = table[0];

            foreach ( ColorData data in table )
            {
                double squaredDistance = SquaredColorDistance( color, data.color );
                
                if ( squaredDistance < closestSquaredDistance )
                {
                    closestSquaredDistance = squaredDistance;
                    closestColor = data;
                }
            }

            return closestColor.name;
        }

        static double SquaredColorDistance( Rgba32 color1, Rgba32 color2 )
        {
            return Math.Pow( color1.R - color2.R, 2 )
                    + Math.Pow( color1.G - color2.G, 2 )
                    + Math.Pow( color1.B - color2.B, 2 )
                    + Math.Pow( color1.A - color2.A, 2 );
        }
    }

}