namespace AppraisalBot
{
    public static class UpdateExpectedAcceptanceTestOutput
    {
        public static void Run()
        {
            TestComposeImageUtils.UpdateExpectedOutput();
            TestImageTransformUtils.UpdateExpectedOutput();
        }
    }
}