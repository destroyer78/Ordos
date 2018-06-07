using Ordos.Core;
using Ordos.DataService;
using Xunit;

namespace Ordos.Tests
{
    public class PathHelperTests
    {
        [Fact]
        public void TestGetExportPath()
        {
            var root = Paths.ExportRoot;
            Assert.NotEmpty(root);
            Assert.Contains("Ordos", root);
        }
    }
}
