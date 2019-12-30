using Xunit;
using System.Reflection;

namespace AppraisalBot
{
    public static class TestUtils
    {
        [Fact]
        public static void TestIsRunningTests()
        {
            Assert.True(Program.IsRunningTests());
        }
    }
}