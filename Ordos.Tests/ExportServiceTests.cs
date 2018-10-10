using Ordos.Core.Models;
using Ordos.DataService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ordos.DataService.Data;
using Xunit;

namespace Ordos.Tests
{
    public class ExportServiceTests
    {
        [Fact]
        public void TestExportDatabaseDRs()
        {
            var drCount = 0;
            var exportPath = "exportTestFolder/";

            Directory.Delete(exportPath,true);
            Assert.False(Directory.Exists(exportPath));

            using (var context = new SystemContext())
            {
                drCount = context.DisturbanceRecordings.Count();
            }

            ExportService.ExportDisturbanceRecordings(exportPath, true);

            Assert.True(Directory.Exists(exportPath));
            Assert.Equal(drCount, new DirectoryInfo(exportPath).GetFiles().Length);
        }

        [Fact]
        public void TestGetZipFileString()
        {
            var exportDeviceName = "testDevice";
            var exportDeviceBayId = "testBayId";
            var exportDateTime = new DateTime(2018, 5, 25, 14, 30, 46);
            Assert.Equal($"20180525,143046000,{exportDeviceBayId},{exportDeviceName}.zip",
                ExportService.GetZipFileName(exportDeviceName, exportDeviceBayId, exportDateTime));
        }

        [Fact]
        public void TestGetZipFileObjects()
        {
            var exportDeviceName = "testDevice";
            var exportDeviceBay = "testBay";
            var exportDeviceBayId = "testBayId";
            var exportDateTime = new DateTime(2018, 5, 25, 14, 30, 46);

            var device = new Device()
            {
                Bay = exportDeviceBay,
                BayId = exportDeviceBayId,
                Name = exportDeviceName,
            };

            var dr = new DisturbanceRecording()
            {
                TriggerTime = exportDateTime,
            };

            Assert.Equal($"20180525,143046000,{exportDeviceBayId},{exportDeviceName}.zip",
                ExportService.GetZipFileName(device, dr));
        }

        [Fact]
        public void TestExportDRsCreateFile()
        {
            var exportPath = "exportTestFolder/";
            var exportDeviceName = "testDevice";
            var exportDeviceBay = "testBay";
            var exportDeviceBayId = "testBayId";
            var exportDateTime = new DateTime(2018, 5, 25, 14, 30, 46);

            var device = new Device()
            {
                Bay = exportDeviceBay,
                BayId = exportDeviceBayId,
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

            ExportService.ExportDisturbanceRecordings(exportPath, exportDeviceName, exportDeviceBayId, exportDeviceBay, list, false);

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
            var exportDeviceBayId = "testBayId";
            var exportDateTime = new DateTime(2018, 5, 25, 14, 30, 46);

            var device = new Device()
            {
                Bay = exportDeviceBay,
                BayId = exportDeviceBayId,
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

            ExportService.ExportDisturbanceRecordings(exportPath, exportDeviceName, exportDeviceBayId, exportDeviceBay, list, false);

            var newSize1 = new FileInfo(fullFilename).Length;

            Assert.Equal(oldSize, newSize1);

            ExportService.ExportDisturbanceRecordings(exportPath, exportDeviceName, exportDeviceBayId, exportDeviceBay, list, true);
            var newSize2 = new FileInfo(fullFilename).Length;

            Assert.NotEqual(oldSize, newSize2);

            Assert.True(File.Exists(fullFilename));

            File.Delete(fullFilename);
            Assert.False(File.Exists(fullFilename));
        }
    }
}
