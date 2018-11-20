using System;
using System.IO;

namespace Ordos.Core
{
    public static class Paths
    {
        public static string DRMFolder => @"Ordos";

        public static string ExportRoot
        {
            get
            {
                // var rootFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var rootFolder = System.IO.Directory.GetCurrentDirectory();
                return Path.Combine(rootFolder, DRMFolder);
            }
        }
    }
}