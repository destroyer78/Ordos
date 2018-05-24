using Ordos.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Ordos.Core.Utilities
{
    public static class ComtradeExtensions
    {
        public static string ComtradeDatetimeFormat { get; } = "dd/MM/yyyy,HH:mm:ss.ffffff";

        public static (bool TryParse, DateTime DateTime) TryParseDRDate(string datetime)
        {
            var tryParse = DateTime.TryParseExact(datetime, ComtradeDatetimeFormat, null, System.Globalization.DateTimeStyles.None, out DateTime result);
            return (tryParse, result);
        }

        public static List<DisturbanceRecording> ParseZipFilesCollection(IEnumerable<FileInfo> fileInfoCollection, int deviceId)
        {
            //Init empty list;
            var disturbanceRecordings = new List<DisturbanceRecording>();

            //Iterate over each zip file.
            //Each zip file should be a DisturbanceRecording;
            foreach (var zipFileInfo in fileInfoCollection)
            {
                using (var zipFile = ZipFile.OpenRead(zipFileInfo.FullName))
                {
                    //Init Empty DR;
                    var dr = new DisturbanceRecording { DeviceId = deviceId };

                    //Parse DR Group:
                    var drFiles = ParseDRZipGroup(zipFile.Entries);

                    if (drFiles.Count > 0)
                    {
                        dr.DRFiles.AddRange(drFiles);
                        dr.Name = drFiles.FirstOrDefault().FileName.GetNameWithoutExtension();
                        dr.TriggerTime = drFiles.FirstOrDefault().CreationTime;

                        //Add the DR to the collection:
                        disturbanceRecordings.Add(dr);
                    }
                }
            }
            return disturbanceRecordings;
        }

        public static IEnumerable<DisturbanceRecording> ParseSingleFilesCollection(IEnumerable<FileInfo> fileInfoCollection, int deviceId)
        {
            //Init empty list;
            var disturbanceRecordings = new List<DisturbanceRecording>();

            //Group files by their name. At least in some Tested IEDs,
            //DR Files all have the same name.
            var drFileGroups = fileInfoCollection.GroupBy(x => x.Name.GetNameWithoutExtension());

            //Iterate over each file group.
            //Each file group should be a DisturbanceRecording;
            foreach (var drFileGroup in drFileGroups)
            {
                //Init Empty DR;
                var dr = new DisturbanceRecording { DeviceId = deviceId };

                //Parse DR Group:
                var drFiles = ParseDRFilesGroup(drFileGroup);

                if (drFiles.Count > 0)
                {
                    dr.DRFiles.AddRange(drFiles);
                    dr.Name = drFileGroup.Key;
                    dr.TriggerTime = drFiles.FirstOrDefault().CreationTime;

                    //Add the DR to the collection:
                    disturbanceRecordings.Add(dr);
                }
            }
            return disturbanceRecordings;
        }

        public static List<DRFile> ParseDRZipGroup(IEnumerable<ZipArchiveEntry> drFilenamesGroup)
        {
            var list = new List<DRFile>();
            drFilenamesGroup = drFilenamesGroup.OrderByDescending(x => x.Name.IsExtension(FileNameExtensions.CFGExtension));

            //Create Temporary variable to store the trigger Time;
            var creationTime = DateTime.Now;

            foreach (var groupItem in drFilenamesGroup.Where(x=>x.Name.IsPartOfDisturbanceRecording()))
            {
                //Add filedata to the DRFile;
                var fileData = Array.Empty<byte>();
                var triggerDate = DateTime.Now;
                using (MemoryStream ms = new MemoryStream())
                {
                    groupItem.Open().CopyTo(ms);
                    fileData = ms.ToArray();
                }

                //Get Comtrade Data;
                if (groupItem.Name.IsExtension(FileNameExtensions.CFGExtension))
                {
                    var cfgContents = ReadLines(groupItem.Open(), Encoding.UTF8);
                    creationTime = GetTriggerDateTime(cfgContents);

                    //TODO: Add TriggerLength; Add TriggerChannel
                    //dr.TriggerLength = ComtradeExtensions.GetDRTriggerLength(cfgContents);
                    //dr.TriggerChannel = ComtradeExtensions.GetDRTriggerChannel(cfgContents);
                }

                //Init the DRFile;
                var drFile = new DRFile
                {
                    FileName = groupItem.Name,
                    FileSize = groupItem.Length,
                    //DisturbanceRecordingId = dr.Id,
                    CreationTime = creationTime,
                    FileData = fileData,
                };
                list.Add(drFile);
            }
            return list;
        }

        public static List<DRFile> ParseDRFilesGroup(IEnumerable<FileInfo> drFilenamesGroup)
        {
            var list = new List<DRFile>();
            drFilenamesGroup = drFilenamesGroup.OrderByDescending(x => x.Name.IsExtension(FileNameExtensions.CFGExtension));

            //Create Temporary variable to store the trigger Time;
            var creationTime = DateTime.Now;

            foreach (var groupItem in drFilenamesGroup)
            {
                //Add filedata to the DRFile;
                var fileData = File.ReadAllBytes(groupItem.FullName);

                //Get Comtrade Data;
                if (groupItem.Name.IsExtension(FileNameExtensions.CFGExtension))
                {
                    creationTime = GetTriggerDateTime(groupItem.FullName);

                    //TODO: Add TriggerLength; Add TriggerChannel
                    //dr.TriggerLength = ComtradeExtensions.GetDRTriggerLength(cfgContents);
                    //dr.TriggerChannel = ComtradeExtensions.GetDRTriggerChannel(cfgContents);
                }

                //Init the DRFile;
                var drFile = new DRFile
                {
                    FileName = groupItem.Name,
                    FileSize = groupItem.Length,
                    //DisturbanceRecordingId = dr.Id,
                    CreationTime = creationTime,
                    FileData = fileData,
                };
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
