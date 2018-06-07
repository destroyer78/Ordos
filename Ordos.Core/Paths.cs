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
                var rootDrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
                return Path.Combine(rootDrive, DRMFolder);
            }
        }
    }
}
