using Ordos.Core.Models;
using Ordos.DataService;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Ordos.Tests
{
    public class ExportServiceTests
    {
        [Fact]
        public void TestGetZipFileString()
        {
            var exportDeviceName = "testDevice";
            var exportDeviceBay = "testBay";
            var exportDateTime = new DateTime(2018, 5, 25, 14, 30, 46);
            Assert.Equal($"20180525,143046000,{exportDeviceBay},{exportDeviceName}.zip",
                DataService.ExportService.GetZipFileName(exportDeviceName, exportDeviceBay, exportDateTime));
        }

        [Fact]
        public void TestGetZipFileObjects()
        {
            var exportDeviceName = "testDevice";
            var exportDeviceBay = "testBay";
            var exportDateTime = new DateTime(2018, 5, 25, 14, 30, 46);

            var device = new Device()
            {
                Bay = exportDeviceBay,
                Name = exportDeviceName,
            };

            var dr = new DisturbanceRecording()
            {
                TriggerTime = exportDateTime,
            };

            Assert.Equal($"20180525,143046000,{exportDeviceBay},{exportDeviceName}.zip",
                DataService.ExportService.GetZipFileName(device, dr));
        }

        [Fact]
        public void TestExportDRsCreateFile()
        {
            var exportPath = "exportTestFolder/";
            var exportDeviceName = "testDevice";
            var exportDeviceBay = "testBay";
            var exportDateTime = new DateTime(2018, 5, 25, 14, 30, 46);

            var device = new Device()
            {
                Bay = exportDeviceBay,
                Name = exportDeviceName,
            };

            var dr = new DisturbanceRecording()
            {
                TriggerTime = exportDateTime,
            };

            var list = new List<DisturbanceRecording>()
            {
                dr,
            };

            var filename = ExportService.GetZipFileName(device, dr);
            var fullFilename = PathHelper.GetOrCreateValidPath(exportPath, filename);

            if (File.Exists(fullFilename))
                File.Delete(fullFilename);

            Assert.False(File.Exists(fullFilename));

            ExportService.ExportDisturbanceRecordings(exportPath, exportDeviceName, exportDeviceBay, list, false);

            Assert.True(File.Exists(fullFilename));

            File.Delete(fullFilename);
            Assert.False(File.Exists(fullFilename));
        }

        [Fact]
        public void TestExportDRsOverWriteFile()
        {
            var exportPath = "exportTestFolder/";
            var exportDeviceName = "testDevice";
            var exportDeviceBay = "testBay";
            var exportDateTime = new DateTime(2018, 5, 25, 14, 30, 46);

            var device = new Device()
            {
                Bay = exportDeviceBay,
                Name = exportDeviceName,
            };

            var drFile = new DRFile()
            {
                FileName = "testFile.cfg",
                FileData = new byte[] { 12, 123, 23, 254 },
            };

            var dr = new DisturbanceRecording()
            {
                TriggerTime = exportDateTime,
                DRFiles = new List<DRFile>()
                {
                    drFile,
                },
            };

            var list = new List<DisturbanceRecording>()
            {
                dr,
            };

            var filename = ExportService.GetZipFileName(device, dr);
            var fullFilename = PathHelper.GetOrCreateValidPath(exportPath, filename);

            if (File.Exists(fullFilename))
                File.Delete(fullFilename);

            Assert.False(File.Exists(fullFilename));
            using (var f = File.Create(fullFilename))
            { }
            Assert.True(File.Exists(fullFilename));

            var oldSize = new FileInfo(fullFilename).Length;

            ExportService.ExportDisturbanceRecordings(exportPath, exportDeviceName, exportDeviceBay, list, false);

            var newSize1 = new FileInfo(fullFilename).Length;

            Assert.Equal(oldSize, newSize1);

            ExportService.ExportDisturbanceRecordings(exportPath, exportDeviceName, exportDeviceBay, list, true);
            var newSize2 = new FileInfo(fullFilename).Length;

            Assert.NotEqual(oldSize, newSize2);

            Assert.True(File.Exists(fullFilename));

            File.Delete(fullFilename);
            Assert.False(File.Exists(fullFilename));
        }
    }
}
