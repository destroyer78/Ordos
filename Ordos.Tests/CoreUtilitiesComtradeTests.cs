using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Ordos.Tests
{
    public class CoreUtilitiesComtradeTests
    {
        //ReadLines
        [Fact]
        public void TestReadLinesNotNull()
        {
            var filename = "./Resources/Single1.CFG";
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                Assert.NotNull(Core.Utilities.ComtradeExtensions.ReadLines(stream, Encoding.UTF8));
        }

        [Fact]
        public void TestReadLinesCount()
        {
            var filename = "./Resources/Single1.CFG";
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                Assert.Equal(73, Core.Utilities.ComtradeExtensions.ReadLines(stream, Encoding.UTF8).ToList().Count);
        }

        [Fact]
        public void TestReadLinesEmpty()
        {
            var filename = "./Resources/EmptyFile.CFG";
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                Assert.Empty(Core.Utilities.ComtradeExtensions.ReadLines(stream, Encoding.UTF8).ToList());
        }

        //GetDRDateTimes
        [Fact]
        public void TestTestGetDRDateTimeEmpty()
        {
            var filename = "./Resources/EmptyFile.CFG";
            Assert.Single(Core.Utilities.ComtradeExtensions.GetDRDateTimes(filename));
        }

        [Fact]
        public void TestGetDRDateTimeCount()
        {
            var filename = "./Resources/Single1.CFG";
            Assert.Equal(2, Core.Utilities.ComtradeExtensions.GetDRDateTimes(filename).Count());
        }

        //GetTriggerDateTime
        [Fact]
        public void TestGetTriggerDateTime()
        {
            var filename = "./Resources/Single1.CFG";
            Assert.Equal(DateTime.Parse("04/04/2018,13:45:38.404284"), Core.Utilities.ComtradeExtensions.GetTriggerDateTime(filename));
        }

        [Fact]
        public void TestGetTriggerDateTimeEmpty()
        {
            var filename = "./Resources/EmptyFile.CFG";
            Assert.Equal(DateTime.Now.ToShortDateString(), Core.Utilities.ComtradeExtensions.GetTriggerDateTime(filename).ToShortDateString());
        }
    }
}
