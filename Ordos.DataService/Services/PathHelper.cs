using Ordos.Core.Models;
using System.IO;
using Ordos.Core.Utilities;
using System;

namespace Ordos.DataService.Services
{
    public static class PathHelper
    {
        public static string GetDeviceSpecificFolder(Device device)
        {
            return Path.Combine(DatabaseService.CompanyName, device.Station.CleanFileName(), device.Bay.CleanFileName(), device.Name.CleanFileName(), "Oscilografias");
        }

        public static string GetDeviceExportFolder(Device device)
        {
            return Path.Combine(PathHelper.ExportRoot, GetDeviceSpecificFolder(device));
        }

        public static string GetDeviceExportPath(Device device, string filename)
        {
            var path = GetDeviceExportFolder(device);
            return PathHelper.ValidatePath(path, filename);
        }

        private static string DRMFolder => @"Ordos";

        public static string ExportRoot
        {
            get
            {
                var rootDrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
                return Path.Combine(rootDrive, DRMFolder);
            }
        }

        public static string ValidatePath(string path, string filename)
        {
            var fileInfo = new FileInfo(Path.Combine(path, filename));

            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                fileInfo.Directory.Create();

            return fileInfo.FullName;
        }

        public static string GetTemporaryDownloadFolder(Device device)
        {
            return Path.Combine(Path.GetTempPath(), DRMFolder, device.Station.CleanFileName(), device.Bay.CleanFileName(), device.Name.CleanFileName());
        }

        public static string GetTemporaryDownloadPath(Device device, string filename)
        {
            var path = GetTemporaryDownloadFolder(device);
            return ValidatePath(path, filename);
        }

        /// <summary>
        /// Delete the donwloaded file folder and recursive files
        /// </summary>
        /// <param name="device">Device to take the Station, Bay, DeviceName</param>
        public static void RemoveTemporaryFiles(Device device)
        {
            var temporaryFolder = GetTemporaryDownloadFolder(device);
            Directory.Delete(temporaryFolder, true);
        }
    }
}
