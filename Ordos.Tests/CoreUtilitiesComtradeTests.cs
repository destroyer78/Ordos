using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Xunit;
using Ordos.Core.Utilities;
using System.Collections.Generic;

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
                Assert.NotNull(ComtradeExtensions.ReadLines(stream, Encoding.UTF8));
        }

        [Fact]
        public void TestReadLinesCount()
        {
            var filename = "./Resources/Single1.CFG";
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                Assert.Equal(73, ComtradeExtensions.ReadLines(stream, Encoding.UTF8).ToList().Count);
        }

        [Fact]
        public void TestReadLinesEmpty()
        {
            var filename = "./Resources/EmptyFile.CFG";
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                Assert.Empty(ComtradeExtensions.ReadLines(stream, Encoding.UTF8).ToList());
        }

        //GetDRDateTimes
        [Fact]
        public void TestTestGetDRDateTimeEmpty()
        {
            var filename = "./Resources/EmptyFile.CFG";
            Assert.Single(ComtradeExtensions.GetDRDateTimes(filename));
        }

        [Fact]
        public void TestGetDRDateTimeCount()
        {
            var filename = "./Resources/Single1.CFG";
            Assert.Equal(2, ComtradeExtensions.GetDRDateTimes(filename).Count());
        }

        //GetTriggerDateTime
        [Fact]
        public void TestGetTriggerDateTime()
        {
            var filename = "./Resources/Single1.CFG";
            Assert.Equal(ComtradeExtensions.TryParseDRDate("05/04/2018,13:45:38.404284").DateTime, ComtradeExtensions.GetTriggerDateTime(filename));
            Assert.Equal(5, ComtradeExtensions.GetTriggerDateTime(filename).Day);
            Assert.Equal(4, ComtradeExtensions.GetTriggerDateTime(filename).Month);
        }

        [Fact]
        public void TestGetTriggerDateTimeEmpty()
        {
            var filename = "./Resources/EmptyFile.CFG";
            Assert.Equal(DateTime.Now.ToShortDateString(), ComtradeExtensions.GetTriggerDateTime(filename).ToShortDateString());
        }

        //Parse Zip File
        [Fact]
        public void TestParseDRZipGroup1()
        {
            var filename = "./Resources/Zip1.zip";
            using (var zipFile = ZipFile.OpenRead(filename))
            {
                var zipEntries = zipFile.Entries;

                Assert.NotEmpty(ComtradeExtensions.ParseDRZipGroup(zipEntries));
                Assert.Equal(2, ComtradeExtensions.ParseDRZipGroup(zipEntries).Count());

                Assert.True(ComtradeExtensions.ParseDRZipGroup(zipEntries).FirstOrDefault().FileSize > 0);

                Assert.NotEmpty(ComtradeExtensions.ParseDRZipGroup(zipEntries).FirstOrDefault().FileName);
                Assert.Equal("010A0005.CFG", ComtradeExtensions.ParseDRZipGroup(zipEntries).FirstOrDefault().FileName);

                Assert.NotEmpty(ComtradeExtensions.ParseDRZipGroup(zipEntries).FirstOrDefault().FileData);
                Assert.Equal(1265, ComtradeExtensions.ParseDRZipGroup(zipEntries).FirstOrDefault().FileData.Length);

                Assert.Equal(ComtradeExtensions.TryParseDRDate("04/04/2018,13:45:38.404284").DateTime, ComtradeExtensions.ParseDRZipGroup(zipEntries).FirstOrDefault().CreationTime);
            }
        }

        [Fact]
        public void TestParseDRZipGroup2()
        {
            var filename = "./Resources/Zip2.zip";
            using (var zipFile = ZipFile.OpenRead(filename))
            {
                var zipEntries = zipFile.Entries;

                Assert.NotEmpty(ComtradeExtensions.ParseDRZipGroup(zipEntries));
                Assert.Equal(3, ComtradeExtensions.ParseDRZipGroup(zipEntries).Count());

                Assert.True(ComtradeExtensions.ParseDRZipGroup(zipEntries).FirstOrDefault().FileSize > 0);

                Assert.NotEmpty(ComtradeExtensions.ParseDRZipGroup(zipEntries).FirstOrDefault().FileName);
                Assert.Equal("Dist.cfg", ComtradeExtensions.ParseDRZipGroup(zipEntries).FirstOrDefault().FileName);

                Assert.NotEmpty(ComtradeExtensions.ParseDRZipGroup(zipEntries).FirstOrDefault().FileData);
                Assert.Equal(1269, ComtradeExtensions.ParseDRZipGroup(zipEntries).FirstOrDefault().FileData.Length);

                Assert.Equal(ComtradeExtensions.TryParseDRDate("20/07/2016,10:09:14.760712").DateTime, ComtradeExtensions.ParseDRZipGroup(zipEntries).FirstOrDefault().CreationTime);
            }
        }

        //Parse File Group
        [Fact]
        public void TestParseFilesGroup()
        {
            var filenames = new List<FileInfo>() {
                new FileInfo("./Resources/Single1.CFG"),
                new FileInfo("./Resources/Single1.DAT")
            };

            Assert.NotEmpty(ComtradeExtensions.ParseDRFilesGroup(filenames));
            Assert.Equal(2, ComtradeExtensions.ParseDRFilesGroup(filenames).Count());

            Assert.True(ComtradeExtensions.ParseDRFilesGroup(filenames).FirstOrDefault().FileSize > 0);

            Assert.NotEmpty(ComtradeExtensions.ParseDRFilesGroup(filenames).FirstOrDefault().FileName);
            Assert.Equal("Single1.CFG", ComtradeExtensions.ParseDRFilesGroup(filenames).FirstOrDefault().FileName);
            Assert.Equal("Single1.DAT", ComtradeExtensions.ParseDRFilesGroup(filenames).LastOrDefault().FileName);

            Assert.NotEmpty(ComtradeExtensions.ParseDRFilesGroup(filenames).FirstOrDefault().FileData);
            Assert.Equal(1265, ComtradeExtensions.ParseDRFilesGroup(filenames).FirstOrDefault().FileData.Length);

            Assert.Equal(ComtradeExtensions.TryParseDRDate("05/04/2018,13:45:38.404284").DateTime, ComtradeExtensions.ParseDRFilesGroup(filenames).FirstOrDefault().CreationTime);
        }
    }
}
