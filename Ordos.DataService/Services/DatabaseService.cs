using Ordos.DataService.Data;
using Ordos.Core.Models;
using Ordos.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ordos.DataService.Services
{
    public static class DatabaseService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static List<Device> Devices { get; private set; }
        public static string CompanyName { get; set; }
        public static string CompanyNameLabel { get; } = "CompanyName";

        public static void Init()
        {
            LoadDevices();
            LoadApplicationSettings();
        }

        public static void LoadDevices()
        {
            using (var context = new SystemContext())
            {
                Devices = context.Devices
                                 .Include(d => d.DisturbanceRecordings).AsNoTracking()
                                 .ToList();
            }
        }

        public static void LoadApplicationSettings()
        {
            using (var context = new SystemContext())
            {
                if (context.ConfigurationValues == null) return;
                CompanyName = context.ConfigurationValues.FirstOrDefault(x => x.Id.Contains(CompanyNameLabel))?.Value;
            }
        }

        public static void UpdateIEDConnectionStatus(Device device, bool isConnected)
        {
            try
            {
                using (var context = new SystemContext())
                {
                    var dev = context.Devices.FirstOrDefault(x => x.Id.Equals(device.Id));
                    if (dev == null) return;

                    dev.IsConnected = isConnected;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            Logger.Trace($"{device.FullName} - {device.IPAddress} - Connection {(isConnected ? "Successful" : "Failed")}");
        }

        public static void StoreComtradeFilesToDatabase(Device device, List<DisturbanceRecording> comtradeFiles)
        {
            using (var context = new SystemContext())
            {
                //Get the DB device:
                var dev = context.Devices
                                 .Include(ied => ied.DisturbanceRecordings)
                                    .ThenInclude(dr => dr.DRFiles)
                                 .FirstOrDefault(x => x.Id.Equals(device.Id));

                //If device not found, return empty list:
                if (dev == null)
                {
                    Logger.Error($"{device} Not found on the DB");
                    return;
                }

                foreach (var item in comtradeFiles)
                {
                    Logger.Trace($"{device} - {item}");
                    dev.DisturbanceRecordings.Add(item);
                }

                context.SaveChanges();
            }
        }

        public static IEnumerable<(string, ulong, uint)> FilterExistingFiles(Device device, IEnumerable<(string FileName, ulong CreationTime, uint FileSize)> downloadableFileList)
        {
            var filteredDownloadableFileList = new List<(string FileName, ulong LastModifiedDate, uint FileSize)>();

            using (var context = new SystemContext())
            {
                //Get the DB device:
                var dev = context.Devices
                                 .Include(x => x.DisturbanceRecordings)
                                    .ThenInclude(x => x.DRFiles)
                                 .FirstOrDefault(x => x.Id.Equals(device.Id));

                //If device not found, return empty list:
                if (dev == null)
                {
                    Logger.Error($"{device} Not found on the DB");
                    return filteredDownloadableFileList;
                }

                //Get the list of all DRFiles in the downloadableFileList:
                //If the ied already has that file (file.name && file.size) (should have it in the database), skip;
                //otherwise add that file to the filtered download list:
                var drFiles = dev.DisturbanceRecordings.SelectMany(x => x.DRFiles);

                foreach (var downloadableFile in downloadableFileList)
                {
                    Logger.Trace($"{device} - {downloadableFile}");

                    if (drFiles
                        .Any(x => x.FileName.Equals(downloadableFile.FileName.GetDestinationFilename())
                             && x.FileSize == downloadableFile.FileSize))
                    {
                        Logger.Trace($"{device} - File already in the DB");
                        continue;
                    }

                    Logger.Trace($"{device} - New file found!");

                    filteredDownloadableFileList.Add(downloadableFile);
                }
            }
            return filteredDownloadableFileList;
        }
    }
}
