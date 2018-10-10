using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ordos.Core
{
    public static class Paths
    {
        public static string DRMFolder => @"Ordos";

        public static string ExportRoot
        {
            get
            {
                var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(userProfilePath, DRMFolder);
            }
        }
    }
}
