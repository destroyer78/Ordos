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
                var rootDrive = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(rootDrive, DRMFolder);
            }
        }
    }
}