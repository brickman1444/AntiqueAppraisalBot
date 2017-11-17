using System;

using SixLabors.ImageSharp;

namespace AppraisalBot
{
    static class ColorTable
    {
        static Rgba32[] table = {
            Rgba32.Red,
            Rgba32.Blue,
            Rgba32.Green,

            Rgba32.Cyan,
            Rgba32.Magenta,
            Rgba32.Yellow,

            Rgba32.Black,
            Rgba32.DimGray,
            Rgba32.Gray,
            Rgba32.LightGray,
            Rgba32.White,

            Rgba32.Orange,
            Rgba32.SaddleBrown,
            Rgba32.Sienna,
            Rgba32.Tan,
            Rgba32.Purple,
            Rgba32.Pink,
        };

        public static Rgba32 GetColorFromHexString( string hexString )
        {   
            int r = Convert.ToInt32(hexString.Substring(0,2), 16);
            int g = Convert.ToInt32(hexString.Substring(2,2), 16);
            int b = Convert.ToInt32(hexString.Substring(4,2), 16);
            return new Rgba32( r, g, b );
        }

        public static string GetClosestColorName( Rgba32 color )
        {
            double closestSquaredDistance = SquaredColorDistance( color, table[0] );
            Rgba32 closestColor = table[0];

            foreach ( Rgba32 c in table )
            {
                double squaredDistance = SquaredColorDistance( color, c );
                
                if ( squaredDistance < closestSquaredDistance )
                {
                    closestSquaredDistance = squaredDistance;
                    closestColor = c;
                }
            }

            string name = GetColorName( closestColor );

            return name;
        }

        static double SquaredColorDistance( Rgba32 color1, Rgba32 color2 )
        {
            return Math.Pow( color1.R - color2.R, 2 )
                    + Math.Pow( color1.G - color2.G, 2 )
                    + Math.Pow( color1.B - color2.B, 2 )
                    + Math.Pow( color1.A - color2.A, 2 );
        }

        public static string Name(this Rgba32 color)
        {
            // TODO: since Rgba32 doesn't have a built in name, the names will have to be manually added to the table
            return "";
        }
        static string GetColorName( Rgba32 color )
        {
            string name = color.Name().ToLower();

            // fix up bad names
            if ( name == "dimgray")
            {
                // This is because dim gray is darker than gray
                name = "dark gray";
            }
            else if( name == "lightgray" )
            {
                name = "light gray";
            }
            else if( name == "saddlebrown" )
            {
                name = "brown";
            }
            else if( name == "sienna")
            {
                name = "light brown";
            }

            return name;
        }
    }

}