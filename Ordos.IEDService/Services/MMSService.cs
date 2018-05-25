using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Ordos.DataService.Services;
using Ordos.Core.Models;
using Ordos.Core.Utilities;
using IEC61850.Client;
using Microsoft.EntityFrameworkCore;
using Ordos.DataService.Data;
using System.Net.NetworkInformation;

namespace Ordos.IEDService.Services
{
    public class MMSService : IConnectionService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Main entry point for the IED Service.
        /// </summary>
        public static void GetComtrades()
        {
            DatabaseService.LoadDevices();

            foreach (var device in DatabaseService.Devices)
            {
                //TODO: Possible issue: During Foreach, user removes IED or adds a new one;
                try
                {
                    if (!TestConnection(device)) continue;
                    ProcessDeviceComtradeFiles(device);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        private static bool TestConnection(Device device)
        {
            Logger.Trace($"{device}");

            bool isConnected = false;
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send(device.IPAddress);
                isConnected = reply.Status == IPStatus.Success;
            }
            catch (PingException e)
            {
                Logger.Error(e);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                DatabaseService.UpdateIEDConnectionStatus(device, isConnected);
            }
            return isConnected;
        }

        /// <summary>
        /// Connect and read IED Comtrade Files.
        /// </summary>
        /// <param name="device">Device to read</param>
        private static void ProcessDeviceComtradeFiles(Device device)
        {
            var iedConnection = new IedConnection
            {
                ConnectTimeout = 20000
            };

            try
            {
                Logger.Info($"{device} - Connecting to Device");

                iedConnection.Connect(device.IPAddress);

                Logger.Info($"{device} - Connection Successful");

                Logger.Info($"{device} - Get IED Files");

                //Download a list of files in the IED:
                //Uses string.Empty as main root of the IED.
                var downloadableFileList = GetDownloadableFileList(iedConnection, device, string.Empty);

                Logger.Info($"{device} - {downloadableFileList.Count()} Files Found");

                //Remove the files that have been already downloaded before:
                //It will filter the files that are not in the DB.
                var filteredDownloadableFileList = FilterExistingFiles(device, downloadableFileList);

                Logger.Info($"{device} - {filteredDownloadableFileList.Count()} New Files Found");

                if (filteredDownloadableFileList.Count() > 0)
                {
                    Logger.Info($"{device} - Downloading Comtrade files");

                    DownloadComtradeFiles(iedConnection, device, filteredDownloadableFileList);

                    Logger.Info($"{device} - Reading files");

                    var temporaryComtradeFiles = ParseTemporaryComtradeFiles(device);

                    Logger.Info($"{device} - Saving files to the DB");

                    StoreComtradeFilesToDatabase(device, temporaryComtradeFiles);

                    Logger.Info($"{device} - Exporting files");

                    ExportDisturbanceRecordings(device, temporaryComtradeFiles);

                    Logger.Info($"{device} - Removing temporary files");

                    PathHelper.RemoveTemporaryFiles(device);
                }
                Logger.Info($"{device} - Disconnecting...");

                //Close connection:
                iedConnection.Release();
            }
            catch (IedConnectionException e)
            {
                Logger.Fatal($"Client Error: {e.GetIedClientError()}");
                Logger.Fatal($"Error Code: {e.GetErrorCode()}");
                Logger.Fatal($"IED Connection Exception: {e}");
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
            }
            finally
            {
                try
                {
                    //libIEC61850: Dispose connection after use.
                    iedConnection.Dispose();
                }
                catch (IedConnectionException e)
                {
                    Logger.Fatal($"Dispose Client Error: {e.GetIedClientError()}");
                    Logger.Fatal($"Dispose Error Code: {e.GetErrorCode()}");
                    Logger.Fatal($"Dispose IED Connection Exception: {e}");
                }
                catch (Exception e)
                {
                    Logger.Fatal(e);
                }
                Logger.Info($"{device} - Disconnected!");
            }
        }

        private static IEnumerable<(string, ulong, uint)> GetDownloadableFileList(IedConnection iedConnection, Device device, string directoryName)
        {
            //Get Files from Device:
            var files = iedConnection.GetFileDirectory(directoryName);

            var downloadableFileList = new List<(string FileName, ulong CreationTime, uint FileSize)>();

            var logDirectoryName = string.IsNullOrWhiteSpace(directoryName) ? "Root" : directoryName;
            Logger.Trace($"{device} - List IED Files on: {logDirectoryName}");

            //Foreach file in the Device:
            //Check if file is a directoty or a file
            //If directory, recursive call with directory name
            //If file, and a valid download extension, add file to downloadable list
            foreach (var file in files)
            {
                var filename = file.GetFileName();

                Logger.Trace($"{device} - {directoryName}{filename}");

                if (filename.IsDirectory())
                {
                    downloadableFileList.AddRange(GetDownloadableFileList(iedConnection, device, directoryName + filename));
                }
                else
                {
                    if (filename.IsDownloadable())
                        downloadableFileList.Add((directoryName + filename, file.GetLastModified(), file.GetFileSize()));
                }
            }
            return downloadableFileList;
        }

        private static IEnumerable<(string, ulong, uint)> FilterExistingFiles(Device device, IEnumerable<(string FileName, ulong CreationTime, uint FileSize)> downloadableFileList)
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

        /// <summary>
        /// Download each file in the download list.
        /// </summary>
        /// <param name="iedConnection"></param>
        /// <param name="device"></param>
        /// <param name="downloadableFileList"></param>
        private static void DownloadComtradeFiles(IedConnection iedConnection, Device device, IEnumerable<(string fileName, ulong creationTime, uint fileSize)> downloadableFileList)
        {
            foreach (var (fileName, creationTime, fileSize) in downloadableFileList)
            {
                Logger.Info($"{device} - Downloading file: {fileName} ({fileSize})");
                //TODO: Check if the GetTemporaryDownloadPath works on both IEDs: 670, 615;
                //var destinationFilename = PathService.GetTemporaryDownloadPath(device, FileName.ReplaceAltDirectorySeparator().CleanFileName());
                var destinationFilename = PathHelper.GetTemporaryDownloadPath(device, fileName.GetDestinationFilename());

                using (var fs = new FileStream(destinationFilename, FileMode.Create, FileAccess.ReadWrite))
                using (var w = new BinaryWriter(fs))
                {
                    iedConnection.GetFile(fileName, GetFileHandler, w);
                }
            }
        }

        private static bool GetFileHandler(object parameter, byte[] data)
        {
            Logger.Trace($"Received {data.Length} bytes");
            BinaryWriter binWriter = (BinaryWriter)parameter;
            binWriter.Write(data);
            return true;
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
        private static List<DisturbanceRecording> ParseTemporaryComtradeFiles(Device device)
        {
            var disturbanceRecordings = new List<DisturbanceRecording>();

            Logger.Trace($"{device}");

            //Using the recently donwloaded files:
            //(Each temporary folder is unique for each IED)
            var temporaryFolder = PathHelper.GetTemporaryDownloadFolder(device);

            //Every DR Zip file that contains multiple Single files:
            disturbanceRecordings.AddRange(ParseTemporaryZipFiles(temporaryFolder, device.Id));

            //And every single file:
            disturbanceRecordings.AddRange(ParseTemporarySingleFiles(temporaryFolder, device.Id));

            //TODO: Test, pero en todo caso, sacar todos los que tengan un size = 0;
            //Un caso particular es que no se descarga el contenido de las DR, el archivo queda en 0;
            disturbanceRecordings.RemoveAll(x => x.DRFiles.Any(y => y.FileSize.Equals(0)));

            return disturbanceRecordings;
        }

        private static List<DisturbanceRecording> ParseTemporaryZipFiles(string temporaryFolder, int deviceId)
        {
            //Get zip files Collection;
            var zipFileList = new DirectoryInfo(temporaryFolder)
                .EnumerateFiles("*.zip", SearchOption.AllDirectories);

            return ComtradeExtensions.ParseZipFilesCollection(zipFileList, deviceId);
        }

        private static IEnumerable<DisturbanceRecording> ParseTemporarySingleFiles(string temporaryFolder, int deviceId)
        {
            //Get all non-ZIP files Collection;
            var drFileList = new DirectoryInfo(temporaryFolder)
                                    .EnumerateFiles("*.*", SearchOption.AllDirectories)
                                    .Where(x => x.Name.IsPartOfDisturbanceRecording());

            return ComtradeExtensions.ParseSingleFilesCollection(drFileList, deviceId);
        }

        private static void StoreComtradeFilesToDatabase(Device device, List<DisturbanceRecording> temporaryComtradeFiles)
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

                foreach (var item in temporaryComtradeFiles)
                {
                    Logger.Trace($"{device} - {item}");
                    dev.DisturbanceRecordings.Add(item);
                }

                context.SaveChanges();
            }
        }

        private static void ExportDisturbanceRecordings(Device device, List<DisturbanceRecording> temporaryComtradeFiles, bool overwriteExisting = false)
        {
            var exportPath = PathHelper.GetDeviceExportFolder(device);

            foreach (var item in temporaryComtradeFiles)
            {
                Logger.Trace($"Export DR: {device} - {item}");

                var zipFilename = $"{item.TriggerTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture)},{item.TriggerTime.ToString("hhmmssfff", CultureInfo.InvariantCulture)},{device.Bay},{device.Name}.zip";
                var zipFileInfo = new FileInfo(PathHelper.ValidatePath(exportPath, zipFilename));

                if (zipFileInfo.Exists || !overwriteExisting)
                {
                    Logger.Trace($"{device} - Zip file exists: {zipFilename}");
                    continue;
                }

                Logger.Trace($"{device} - Creating Zip file: {zipFilename}");

                using (ZipArchive zip = ZipFile.Open(zipFileInfo.FullName, ZipArchiveMode.Update))
                {
                    foreach (var drFile in item.DRFiles)
                    {
                        Logger.Trace($"{device} - Adding {drFile} to Zip file: {zipFilename}");

                        var fileInZip = zip.Entries.Where(x => x.Name.Equals(drFile.FileName)).FirstOrDefault();

                        ZipArchiveEntry zipEntry;

                        if (fileInZip != null)
                        {
                            fileInZip.Delete();
                            zipEntry = zip.CreateEntry(drFile.FileName);
                        }
                        else
                        {
                            zipEntry = zip.CreateEntry(drFile.FileName);
                        }

                        //Get the stream of the attachment
                        using (var originalFileStream = new MemoryStream(drFile.FileData))
                        using (var zipEntryStream = zipEntry.Open())
                        {
                            //Copy the attachment stream to the zip entry stream
                            originalFileStream.CopyTo(zipEntryStream);
                        }
                    }
                }
            }
        }

        void IConnectionService.GetComtrades()
        {
            GetComtrades();
        }

        bool IConnectionService.TestConnection(Device device)
        {
            return TestConnection(device);
        }
    }
}
