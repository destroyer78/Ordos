using Ordos.Core.Models;
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

        public static void ExportDisturbanceRecordings(Device device, List<DisturbanceRecording> temporaryComtradeFiles, bool overwriteExisting = false)
        {
            var exportPath = PathHelper.GetDeviceExportFolder(device);

            foreach (var item in temporaryComtradeFiles)
            {
                Logger.Trace($"Export DR: {device} - {item}");

                var zipFilename = $"{item.TriggerTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture)},{item.TriggerTime.ToString("hhmmssfff", CultureInfo.InvariantCulture)},{device.Bay},{device.Name}.zip";
                var zipFileInfo = new FileInfo(PathHelper.GetOrCreateValidPath(exportPath, zipFilename));

                if (zipFileInfo.Exists || !overwriteExisting)
                {
                    Logger.Trace($"{device} - Zip file exists: {zipFilename}");
                    continue;
                }

                Logger.Trace($"{device} - Creating Zip file: {zipFilename}");

                using (ZipArchive zip = ZipFile.Open(zipFileInfo.FullName, ZipArchiveMode.Update))
                {
                    foreach (var drFile in item.DRFiles)
                    {
                        Logger.Trace($"{device} - Adding {drFile} to Zip file: {zipFilename}");

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
        }
    }
}
