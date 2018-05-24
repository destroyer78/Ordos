using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ordos.Tests
{
    public class CoreUtilitiesFilenameExtensionsTests
    {
        //GetDestinationFilename
        [Fact]
        public void TestGetDestinationFilenameNotNull()
        {
            var filename = "./Resources/Single1.CFG";
            Assert.NotNull(Core.Utilities.FileNameExtensions.GetDestinationFilename(filename));
            Assert.Equal("Single1.CFG", Core.Utilities.FileNameExtensions.GetDestinationFilename(filename));
        }

        //IsExtension
        [Fact]
        public void TestIsExtension()
        {
            var filename = "./Resources/Single1.CFG";
            Assert.True(Core.Utilities.FileNameExtensions.IsExtension(filename, ".cfg"));
            Assert.True(Core.Utilities.FileNameExtensions.IsExtension(filename, ".CFG"));
        }

        //IsDownloadable
        [Fact]
        public void TestIsDownloadable()
        {
            Assert.True(Core.Utilities.FileNameExtensions.IsDownloadable("./Resources/Single1.CFG"));
            Assert.True(Core.Utilities.FileNameExtensions.IsDownloadable("./Resources/Single1.DAT"));
            Assert.True(Core.Utilities.FileNameExtensions.IsDownloadable("./Resources/Zip1.zip"));

            Assert.False(Core.Utilities.FileNameExtensions.IsDownloadable("./Resources/Text1.txt"));
            Assert.False(Core.Utilities.FileNameExtensions.IsDownloadable("./Resources/Zip1h.zip"));
        }

        //IsDirectory
        [Fact]
        public void TestIsDirectory()
        {
            Assert.True(Core.Utilities.FileNameExtensions.IsDirectory("./Resources/"));
            Assert.True(Core.Utilities.FileNameExtensions.IsDirectory("./Resources\\"));

            Assert.False(Core.Utilities.FileNameExtensions.IsDirectory("./Resources/Text1.txt"));
            Assert.False(Core.Utilities.FileNameExtensions.IsDirectory("./Resources/Zip1h.zip"));
        }

        //GetNameWithoutExtension
        [Fact]
        public void TestGetNameWithoutExtension()
        {
            Assert.Equal("Single1", Core.Utilities.FileNameExtensions.GetNameWithoutExtension("./Resources/Single1.CFG"));
        }

        //CleanFileName
        [Fact]
        public void TestCleanFileName()
        {
            Assert.Equal("Single1.CFG", Core.Utilities.FileNameExtensions.CleanFileName("Single1.CFG"));
            Assert.Equal("Single1.CFG", Core.Utilities.FileNameExtensions.CleanFileName("Sin?gle1.CFG"));
            Assert.Equal("Single1.CFG", Core.Utilities.FileNameExtensions.CleanFileName(":Single1.CFG"));
            Assert.Equal("Single1.CFG", Core.Utilities.FileNameExtensions.CleanFileName("/Single1.CFG"));
            Assert.Equal("Single1.CFG", Core.Utilities.FileNameExtensions.CleanFileName("Sin\\gle1.CFG"));
            Assert.Equal("Single1.CFG", Core.Utilities.FileNameExtensions.CleanFileName("Sin*gle1.CFG"));
            Assert.Equal("Single1.CFG", Core.Utilities.FileNameExtensions.CleanFileName("Sin<gle1.CFG"));
            Assert.Equal("Single1.CFG", Core.Utilities.FileNameExtensions.CleanFileName("|Sin<*gle1.CFG"));
            Assert.Equal("Single1.CFG", Core.Utilities.FileNameExtensions.CleanFileName("|Single1.CFG"));
        }
    }
}
