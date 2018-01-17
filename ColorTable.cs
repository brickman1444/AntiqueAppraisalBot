using System;

using SixLabors.ImageSharp;

namespace AppraisalBot
{
    static class ColorTable
    {
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
            new ColorData( Rgba32.Red, "red" ),
            new ColorData( Rgba32.Blue, "blue" ),
            new ColorData( Rgba32.Green, "green" ),

            new ColorData( Rgba32.Cyan, "cyan" ),
            new ColorData( Rgba32.Magenta, "magenta" ),
            new ColorData( Rgba32.Yellow, "yellow" ),

            new ColorData( Rgba32.Black, "black" ),
            new ColorData( Rgba32.DimGray, "dark gray" ),
            new ColorData( Rgba32.Gray, "gray" ),
            new ColorData( Rgba32.LightGray, "light gray" ),
            new ColorData( Rgba32.White, "white" ),

            new ColorData( Rgba32.Orange, "orange" ),
            new ColorData( Rgba32.SaddleBrown, "brown" ),
            new ColorData( Rgba32.Sienna, "light brown" ),
            new ColorData( Rgba32.Tan, "tan" ),
            new ColorData( Rgba32.Purple, "purple" ),
            new ColorData( Rgba32.Pink, "pink" ),
        };

        public static Rgba32 GetColorFromHexString( string hexString )
        {
            return Rgba32.FromHex( hexString );
        }

        public static string GetClosestColorName( Rgba32 color )
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