namespace AppraisalBot
{
    public static class TestLoadEveryImage
    {
        public static void Run()
        {
            Program.LoadImage(Program.LoadImageType.Source, "footer.png");
            Program.LoadImage(Program.LoadImageType.Source, "widePaperSource.jpg");
            Program.LoadImage(Program.LoadImageType.Source, "tallPaperSource.jpg");
        }
    }
}