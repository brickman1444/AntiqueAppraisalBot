using System;

using System.Drawing;

namespace AppraisalBot
{
    class ColorTable
    {
        static Color[] table = {
            Color.Red,
            Color.Blue,
            Color.Green,

            Color.Cyan,
            Color.Magenta,
            Color.Yellow,

            Color.Black,
            Color.DimGray,
            Color.Gray,
            Color.LightGray,
            Color.White,

            Color.Orange,
            Color.SaddleBrown,
            Color.Sienna,
            Color.Tan,
            Color.Purple,
            Color.Pink,
        };

        public static Color GetColorFromHexString( string hexString )
        {   
            int r = Convert.ToInt32(hexString.Substring(0,2), 16);
            int g = Convert.ToInt32(hexString.Substring(2,2), 16);
            int b = Convert.ToInt32(hexString.Substring(4,2), 16);
            return System.Drawing.Color.FromArgb( r, g, b );
        }

        public static string GetClosestColorName( Color color )
        {
            double closestSquaredDistance = SquaredColorDistance( color, table[0] );
            Color closestColor = table[0];

            foreach ( Color c in table )
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

        static double SquaredColorDistance( Color color1, Color color2 )
        {
            return Math.Pow( color1.R - color2.R, 2 )
                    + Math.Pow( color1.G - color2.G, 2 )
                    + Math.Pow( color1.B - color2.B, 2 )
                    + Math.Pow( color1.A - color2.A, 2 );
        }

        static string GetColorName( Color color )
        {
            string name = color.Name.ToLower();

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