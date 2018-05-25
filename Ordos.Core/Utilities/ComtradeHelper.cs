using Ordos.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Ordos.Core.Utilities
{
    public static class ComtradeHelper
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static string ComtradeDatetimeFormat { get; } = "dd/MM/yyyy,HH:mm:ss.ffffff";

        public static (bool TryParse, DateTime DateTime) TryParseDRDate(string datetime)
        {
            var tryParse = DateTime.TryParseExact(datetime, ComtradeDatetimeFormat, null, System.Globalization.DateTimeStyles.None, out DateTime result);
            return (tryParse, result);
        }

        /// <summary>
        /// Will parse:
        /// -> Read
        /// -> Extract Trigger Date
        /// -> Extract Trigger Length
        /// -> Extract Trigger Channel
        /// -> Group into a DisturbanceRecording
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static List<DisturbanceRecording> ParseComtradeFiles(Device device, string comtradeFileFolderPath)
        {
            var disturbanceRecordings = new List<DisturbanceRecording>();

            Logger.Trace($"{device}");

            //Every DR Zip file that contains multiple Single files:
            var zipFileList = new DirectoryInfo(comtradeFileFolderPath)
                .EnumerateFiles("*.zip", SearchOption.AllDirectories);

            disturbanceRecordings.AddRange(ParseZipFilesCollection(zipFileList, device.Id));

            //Get all non-ZIP files Collection (Single files);
            var drFileList = new DirectoryInfo(comtradeFileFolderPath)
                                    .EnumerateFiles("*.*", SearchOption.AllDirectories)
                                    .Where(x => x.Name.IsPartOfDisturbanceRecording());

            disturbanceRecordings.AddRange(ParseSingleFilesCollection(drFileList, device.Id));

            //TODO: Test, pero en todo caso, sacar todos los que tengan un size = 0;
            //Un caso particular es que no se descarga el contenido de las DR, el archivo queda en 0;
            disturbanceRecordings.RemoveAll(x => x.DRFiles.Any(y => y.FileSize.Equals(0)));

            return disturbanceRecordings;
        }

        public static List<DisturbanceRecording> ParseZipFilesCollection(IEnumerable<FileInfo> fileInfoCollection, int deviceId)
        {
            var disturbanceRecordings = new List<DisturbanceRecording>();

            //Iterate over each ZIP file. Each ZIP file should be a DisturbanceRecording;
            foreach (var zipFileInfo in fileInfoCollection)
            {
                using (var zipFile = ZipFile.OpenRead(zipFileInfo.FullName))
                {
                    var dr = new DisturbanceRecording { DeviceId = deviceId };

                    //Parse DR Group:
                    var drFiles = ParseDRZipGroup(zipFile.Entries);

                    //Only create a dr if it has any files on it
                    if (drFiles.Count > 0)
                    {
                        dr.DRFiles.AddRange(drFiles);

                        //When zipFile, internal names could be anything. Use zipfile instead.
                        dr.Name = zipFileInfo.Name.GetNameWithoutExtension();

                        //They all (should) have the same date
                        dr.TriggerTime = drFiles.FirstOrDefault().CreationTime;

                        Logger.Trace($"{dr}");

                        disturbanceRecordings.Add(dr);
                    }
                }
            }
            return disturbanceRecordings;
        }

        public static IEnumerable<DisturbanceRecording> ParseSingleFilesCollection(IEnumerable<FileInfo> fileInfoCollection, int deviceId)
        {
            var disturbanceRecordings = new List<DisturbanceRecording>();

            //Group files by their name. 
            //When not zipped, DR Files all have the same name.
            var drFileGroups = fileInfoCollection.GroupBy(x => x.Name.GetNameWithoutExtension());

            //Iterate over each file group. Each file group should be a DisturbanceRecording;
            foreach (var drFileGroup in drFileGroups)
            {
                Logger.Trace($"Device Id: {deviceId} - drName: {drFileGroup.Key}");

                var dr = new DisturbanceRecording { DeviceId = deviceId };

                //Parse DR Group:
                var drFiles = ParseDRFilesGroup(drFileGroup);

                //Only create a dr if it has any files on it
                if (drFiles.Count > 0)
                {
                    dr.DRFiles.AddRange(drFiles);

                    //Use Group.Key as DR Name
                    dr.Name = drFileGroup.Key;

                    //They all (should) have the same date
                    dr.TriggerTime = drFiles.FirstOrDefault().CreationTime;

                    Logger.Trace($"{dr}");

                    disturbanceRecordings.Add(dr);
                }
            }
            return disturbanceRecordings;
        }

        public static List<DRFile> ParseDRZipGroup(IEnumerable<ZipArchiveEntry> zippedFiles)
        {
            var list = new List<DRFile>();

            //Only the CFG files contain TriggerTime, parse them first;
            zippedFiles = zippedFiles.OrderByDescending(x => x.Name.IsExtension(FileNameExtensions.CFGExtension));

            //Fallback DateTime
            var creationTime = DateTime.Now;

            foreach (var zipFileEntry in zippedFiles.Where(x=>x.Name.IsPartOfDisturbanceRecording()))
            {
                Logger.Trace($"{zipFileEntry.Name}");

                //Add filedata to the DRFile;
                var fileData = Array.Empty<byte>();
                var triggerDate = DateTime.Now;
                using (MemoryStream ms = new MemoryStream())
                {
                    zipFileEntry.Open().CopyTo(ms);
                    fileData = ms.ToArray();
                }

                //Only the CFG files contain TriggerTime;
                if (zipFileEntry.Name.IsExtension(FileNameExtensions.CFGExtension))
                {
                    var cfgContents = ReadLines(zipFileEntry.Open(), Encoding.UTF8);
                    creationTime = GetTriggerDateTime(cfgContents);
                }

                //Init the DRFile;
                var drFile = new DRFile
                {
                    FileName = zipFileEntry.Name,
                    FileSize = zipFileEntry.Length,
                    //DisturbanceRecordingId = dr.Id,
                    CreationTime = creationTime,
                    FileData = fileData,
                };

                Logger.Trace($"{drFile}");

                list.Add(drFile);
            }
            return list;
        }

        public static List<DRFile> ParseDRFilesGroup(IEnumerable<FileInfo> drFileInfos)
        {
            var list = new List<DRFile>();

            //Only the CFG files contain TriggerTime, parse them first;
            drFileInfos = drFileInfos.OrderByDescending(x => x.Name.IsExtension(FileNameExtensions.CFGExtension));

            //Fallback DateTime
            var creationTime = DateTime.Now;

            foreach (var fileInfo in drFileInfos)
            {
                Logger.Trace($"{fileInfo.Name}");

                //Add filedata to the DRFile;
                var fileData = File.ReadAllBytes(fileInfo.FullName);

                //Only the CFG files contain TriggerTime;
                if (fileInfo.Name.IsExtension(FileNameExtensions.CFGExtension))
                {
                    creationTime = GetTriggerDateTime(fileInfo.FullName);
                }

                //Init the DRFile;
                var drFile = new DRFile
                {
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    //DisturbanceRecordingId = dr.Id,
                    CreationTime = creationTime,
                    FileData = fileData,
                };

                Logger.Trace($"{drFile}");

                list.Add(drFile);
            }
            return list;
        }

        public static DateTime GetTriggerDateTime(string cfgFilename)
        {
            return GetDRDateTimes(cfgFilename).FirstOrDefault();
        }

        public static DateTime GetTriggerDateTime(IEnumerable<string> cfgFileLines)
        {
            return GetDRDateTimes(cfgFileLines).FirstOrDefault();
        }

        public static IEnumerable<DateTime> GetDRDateTimes(IEnumerable<string> cfgFileLines)
        {
            var res = new List<DateTime>();

            foreach (var line in cfgFileLines)
            {
                var (tryParse, result) = TryParseDRDate(line);
                if (tryParse)
                    res.Add(result);
            }
            if (res.Count.Equals(0))
                res.Add(DateTime.Now);
            return res;
        }

        public static IEnumerable<DateTime> GetDRDateTimes(string cfgFilename)
        {
            return GetDRDateTimes(System.IO.File.ReadAllLines(cfgFilename));
        }

        public static IEnumerable<string> ReadLines(Stream stream, Encoding encoding)
        {
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
