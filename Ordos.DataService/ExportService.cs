using Ordos.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Ordos.DataService
{
    public class ExportService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static string GetZipFileName(string deviceName, string deviceBay, DateTime triggerDateTime)
        {
            return $"{triggerDateTime.ToString("yyyyMMdd,HHmmssfff", CultureInfo.InvariantCulture)},{deviceBay},{deviceName}.zip";
        }

        public static string GetZipFileName(Device device, DisturbanceRecording dr)
        {
            return GetZipFileName(device.Name, device.Bay, dr.TriggerTime);
        }

        public static void ExportDisturbanceRecordings(Device device, List<DisturbanceRecording> comtradeFiles, bool overwriteExisting = false)
        {
            var exportPath = PathHelper.GetDeviceExportFolder(device);

            ExportDisturbanceRecordings(exportPath, device.Name, device.Bay, comtradeFiles, overwriteExisting);
        }

        public static void ExportDisturbanceRecordings(string exportPath, string deviceName, string deviceBay, List<DisturbanceRecording> comtradeFiles, bool overwriteExisting = false)
        {
            foreach (var item in comtradeFiles)
            {
                Logger.Trace($"Export DR: {deviceName} - {item}");

                var zipFilename = GetZipFileName(deviceName, deviceBay, item.TriggerTime);
                var zipFileInfo = new FileInfo(PathHelper.GetOrCreateValidPath(exportPath, zipFilename));

                if (zipFileInfo.Exists && !overwriteExisting)
                {
                    Logger.Trace($"{deviceName} - Zip file exists: {zipFilename}");
                    continue;
                }

                Logger.Trace($"{deviceName} - Creating Zip file: {zipFilename}");

                using (ZipArchive zip = ZipFile.Open(zipFileInfo.FullName, ZipArchiveMode.Update))
                {
                    foreach (var drFile in item.DRFiles)
                    {
                        Logger.Trace($"{deviceName} - Adding {drFile} to Zip file: {zipFilename}");
                        AddDRtoZipFile(zip, drFile);
                    }
                }
            }
        }

        private static void AddDRtoZipFile(ZipArchive zip, DRFile drFile)
        {
            var fileInZip = zip.Entries.Where(x => x.Name.Equals(drFile.FileName)).FirstOrDefault();

            ZipArchiveEntry zipEntry;

            if (fileInZip != null)
            {
                fileInZip.Delete();
                zipEntry = zip.CreateEntry(drFile.FileName);
            }
            else
            {
                zipEntry = zip.CreateEntry(drFile.FileName);
            }

            //Get the stream of the attachment
            using (var originalFileStream = new MemoryStream(drFile.FileData))
            using (var zipEntryStream = zipEntry.Open())
            {
                //Copy the attachment stream to the zip entry stream
                originalFileStream.CopyTo(zipEntryStream);
            }
        }
    }
}
