using Ordos.Core.Models;
using System;
using System.IO;
using Ordos.Core.Utilities;
using Ordos.DataService.Services;

namespace Ordos.IEDService.Services
{
    public static class PathService
    {
        private static string DRMFolder => @"Ordos";

        internal static string ExportRoot
        {
            get
            {
                var rootDrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
                return Path.Combine(rootDrive, DRMFolder);
            }
        }

        internal static string GetDeviceSpecificFolder(Device device)
        {
            return Path.Combine(DatabaseService.CompanyName, device.Station.CleanFileName(), device.Bay.CleanFileName(), device.Name.CleanFileName(), "Oscilografias");
        }

        internal static string ValidatePath(string path, string filename)
        {
            var fileInfo = new FileInfo(Path.Combine(path, filename));

            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                fileInfo.Directory.Create();

            return fileInfo.FullName;
        }

        internal static string GetDeviceExportFolder(Device device)
        {
            return Path.Combine(ExportRoot, GetDeviceSpecificFolder(device));
        }

        internal static string GetTemporaryDownloadFolder(Device device)
        {
            return Path.Combine(Path.GetTempPath(), DRMFolder, device.Station.CleanFileName(), device.Bay.CleanFileName(), device.Name.CleanFileName());
        }

        internal static string GetTemporaryDownloadPath(Device device, string filename)
        {
            var path = GetTemporaryDownloadFolder(device);
            return ValidatePath(path, filename);
        }

        internal static string GetDeviceExportPath(Device device, string filename)
        {
            var path = GetDeviceExportFolder(device);
            return ValidatePath(path, filename);
        }
    }
}
