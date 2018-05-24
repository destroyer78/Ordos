using Ordos.IEDService.Services;
using Xunit;

namespace Ordos.Tests
{
    public class PathHelperTests
    {
        [Fact]
        public void TestGetExportPath()
        {
            var root = PathHelper.ExportRoot;
            Assert.NotEmpty(root);
            Assert.Contains("Ordos", root);
        }
    }
}
