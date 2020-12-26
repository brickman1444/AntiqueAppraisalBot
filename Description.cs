using System.Linq;

using Caption = Microsoft.ProjectOxford.Vision.Contract.Caption;

namespace AppraisalBot
{
    public static class Description
    {
        public static string Get(Caption caption, string foregroundColor, bool isOld, bool isBlackAndWhite)
        {
            // Filter and adjust the caption
            string descriptionText = caption.Text;

            string[] stringsToRemove = {
                "a close up of ",
                " that is sitting on a table",
                " sitting on a table",
                " sitting on a counter",
                " on a table",
                "a vintage photo of ",
                " sitting on top of a table",
            };

            foreach (string text in stringsToRemove)
            {
                descriptionText = descriptionText.Replace(text, "");
            }

            // Capitalize the first letter
            descriptionText = char.ToUpper(descriptionText[0]) + descriptionText.Substring(1);

            string[] commonSimpleDescriptions = {
                "A vase",
                "A bowl",
                "A plate",
                "A knife",
                "A clock",
                "A cup of coffee",
                "A bird",
                "A tool",
                "A weapon",
                "A gun",
                "A logo",
                "A box",
                "A sign",
                "A envelope",
                "A pot",
                "A book",
            };

            bool isSimple = commonSimpleDescriptions.Contains(descriptionText);

            if (isSimple)
            {
                if (isBlackAndWhite)
                {
                    if (isOld)
                    {
                        descriptionText = "An old " + descriptionText.Substring(2);
                        System.Console.WriteLine("Added 'old' to simple description");
                    }
                    else
                    {
                        // TODO: Do something clever here?
                        // I don't want to add a color here since I know the image is black and white.
                        // This might be pretty rare since black and white images are often old
                        System.Console.WriteLine("Description was simple but not old. Leaving simple description.");
                    }
                }
                else
                {
                    string color = foregroundColor.ToLower();
                    descriptionText = descriptionText.Substring(0, 2) + color + " " + descriptionText.Substring(2);
                    System.Console.WriteLine("Added color to simple description: " + color);
                }
            }

            return descriptionText;
        }
    }
}