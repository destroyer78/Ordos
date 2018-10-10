using Ordos.Core.Utilities;
using System.IO;
using System.Linq;
using Xunit;

namespace Ordos.Tests
{
    public class LargeFilesetCollectionTests
    {
        //Parse Single Files Folder
        [Fact]
        public void TestParseFilesGroupFolder()
        {
            var dir = @"C:\Users\admin\Desktop\Ordos DRs\Ordos\GIS 23KV\1\";

            if (!Directory.Exists(dir))
                return;

            var drFileList = new DirectoryInfo(dir)
                .EnumerateFiles("*.*", SearchOption.AllDirectories)
                .Where(x => x.Name.IsPartOfDisturbanceRecording());

            var disturbanceRecordings = ComtradeHelper.ParseSingleFilesCollection(drFileList, 1);

            Assert.NotEmpty(disturbanceRecordings);

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
            var dir = @"C:\Users\admin\Desktop\Ordos DRs\Ordos\GIS 23KV\1\";

            if (!Directory.Exists(dir))
                return;

            var drFileList = new DirectoryInfo(dir)
                .EnumerateFiles("*.zip", SearchOption.AllDirectories)
                .Where(x => x.Name.IsDownloadable());

            var disturbanceRecordings = ComtradeHelper.ParseZipFilesCollection(drFileList, 1);

            Assert.NotEmpty(disturbanceRecordings);

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
