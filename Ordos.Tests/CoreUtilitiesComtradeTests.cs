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

                var drFiles = ComtradeExtensions.ParseDRZipGroup(zipEntries);

                Assert.NotEmpty(drFiles);
                Assert.Equal(2, drFiles.Count());

                foreach (var item in drFiles)
                {
                    Assert.True(item.FileSize > 1);

                    Assert.NotEmpty(item.FileName);
                    Assert.Contains("010A0005", item.FileName);
                    Assert.True(item.FileName.Length > 4);

                    Assert.NotEmpty(item.FileData);
                    Assert.True(item.FileData.Length > 1);

                    Assert.Equal(ComtradeExtensions.TryParseDRDate("04/04/2018,13:45:38.404284").DateTime, item.CreationTime);
                }
            }
        }

        [Fact]
        public void TestParseDRZipGroup2()
        {
            var filename = "./Resources/Zip2.zip";
            using (var zipFile = ZipFile.OpenRead(filename))
            {
                var zipEntries = zipFile.Entries;

                var drFiles = ComtradeExtensions.ParseDRZipGroup(zipEntries);

                Assert.NotEmpty(drFiles);
                Assert.Equal(3, drFiles.Count());

                foreach (var item in drFiles)
                {
                    Assert.True(item.FileSize > 1);

                    Assert.NotEmpty(item.FileName);
                    Assert.Contains("Dist", item.FileName);
                    Assert.True(item.FileName.Length > 4);

                    Assert.NotEmpty(item.FileData);
                    Assert.True(item.FileData.Length > 1);

                    Assert.Equal(ComtradeExtensions.TryParseDRDate("20/07/2016,10:09:14.760712").DateTime, item.CreationTime);
                }
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

            var drFiles = ComtradeExtensions.ParseDRFilesGroup(filenames);

            Assert.NotEmpty(drFiles);
            Assert.Equal(2, drFiles.Count());

            foreach (var item in drFiles)
            {
                Assert.True(item.FileSize > 1);

                Assert.NotEmpty(item.FileName);
                Assert.Contains("Single1", item.FileName);
                Assert.True(item.FileName.Length > 4);

                Assert.NotEmpty(item.FileData);
                Assert.True(item.FileData.Length > 1);

                Assert.Equal(ComtradeExtensions.TryParseDRDate("05/04/2018,13:45:38.404284").DateTime, item.CreationTime);
            }
        }

        //Parse Single Files Folder
        [Fact]
        public void TestParseFilesGroupFolder()
        {
            var drFileList = new DirectoryInfo("./Resources/")
                .EnumerateFiles("*.*", SearchOption.AllDirectories)
                .Where(x => !x.Name.Contains("Empty"))
                .Where(x => x.Name.IsPartOfDisturbanceRecording());

            var disturbanceRecordings = ComtradeExtensions.ParseSingleFilesCollection(drFileList, 1);

            Assert.NotEmpty(disturbanceRecordings);
            Assert.Equal(2, disturbanceRecordings.Count());

            foreach (var dr in disturbanceRecordings)
            {
                Assert.NotEmpty(dr.Name);

                var drFiles = dr.DRFiles;

                Assert.NotEmpty(drFiles);
                Assert.True(drFiles.Count() > 1);

                foreach (var item in drFiles)
                {
                    Assert.True(item.FileSize > 1);

                    Assert.NotEmpty(item.FileName);
                    Assert.True(item.FileName.Length > 4);

                    Assert.NotEmpty(item.FileData);
                    Assert.True(item.FileData.Length > 1);

                    Assert.Contains(dr.Name, item.FileName);
                    Assert.Equal(dr.TriggerTime, item.CreationTime);
                }
            }
        }

        //Parse ZIP Files Folder
        [Fact]
        public void TestParseZIPFolder()
        {
            var drFileList = new DirectoryInfo("./Resources/")
                .EnumerateFiles("*.zip", SearchOption.AllDirectories)
                .Where(x => x.Name.IsDownloadable()); 

            var disturbanceRecordings = ComtradeExtensions.ParseZipFilesCollection(drFileList, 1);

            Assert.NotEmpty(disturbanceRecordings);
            Assert.Equal(2, disturbanceRecordings.Count());

            foreach (var dr in disturbanceRecordings)
            {
                Assert.NotEmpty(dr.Name);

                var drFiles = dr.DRFiles;

                Assert.NotEmpty(drFiles);
                Assert.True(drFiles.Count() > 1);

                foreach (var item in drFiles)
                {
                    Assert.True(item.FileSize > 1);

                    Assert.NotEmpty(item.FileName);
                    Assert.True(item.FileName.Length > 4);

                    Assert.NotEmpty(item.FileData);
                    Assert.True(item.FileData.Length > 1);

                    //Assert.Contains(dr.Name, item.FileName);
                    Assert.Equal(dr.TriggerTime, item.CreationTime);
                }
            }
        }
    }
}
