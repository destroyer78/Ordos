﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ordos.Core.Utilities
{
    public static class FileNameExtensions
    {
        private static readonly string[] DownloadableFromDevice = { ".zip", ".cfg", ".dat", ".hdr", ".cff" };
        private static readonly string[] PartsOfDisturbanceRecord = { ".cfg", ".dat", ".hdr" };

        public static string GetDestionationFilename(this string FileName)
        {
            return Path.GetFileName(FileName).CleanFileName();
        }

        private static bool CompareToExtension(this string orginalExtension, string extension)
        {
            return string.Equals(orginalExtension, extension, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsExtension(this string filename, string extension)
        {
            return CompareToExtension(Path.GetExtension(filename), extension);
        }

        public static bool IsDownloadable(this string filename)
        {
            if (filename.ToUpper().Equals("H.ZIP"))
                return false;
            return DownloadableFromDevice.Any(x => CompareToExtension(Path.GetExtension(filename), x));
        }

        public static bool IsPartOfDisturbanceRecording(this string fileExtension)
        {
            return PartsOfDisturbanceRecord.Any(x => CompareToExtension(fileExtension, x));
        }

        public static bool IsDirectory(this string filename)
        {
            return filename.EndsWith(Path.DirectorySeparatorChar.ToString()) || filename.EndsWith(Path.AltDirectorySeparatorChar.ToString());
        }

        public static string GetNameWithoutExtension(this string filename)
        {
            return Path.GetFileNameWithoutExtension(filename);
        }

        public static string ReplaceAltDirectorySeparator(this string filename)
        {
            return filename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public static string CleanFileName(this string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
    }
}