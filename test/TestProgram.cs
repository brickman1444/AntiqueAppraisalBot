using Xunit;
using System.Reflection;

namespace AppraisalBot
{
    public static class TestProgram
    {
        [Fact]
        public static void TestIsRunningTests()
        {
            Assert.True(Program.IsRunningTests());
        }
    }
}