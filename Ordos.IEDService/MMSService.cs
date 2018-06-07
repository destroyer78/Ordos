using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Ordos.DataService;
using Ordos.Core.Models;
using Ordos.Core.Utilities;
using IEC61850.Client;
using Microsoft.EntityFrameworkCore;
using Ordos.DataService.Data;
using System.Net.NetworkInformation;

namespace Ordos.IEDService
{
    public class MMSService : IConnectionService
    {
        private static readonly NLog.Logger Logger = Core.Utilities.Logger.Init();

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
                var filteredDownloadableFileList = DatabaseService.FilterExistingFiles(device, downloadableFileList);

                Logger.Info($"{device} - {filteredDownloadableFileList.Count()} New Files Found");

                if (filteredDownloadableFileList.Count() > 0)
                {
                    Logger.Info($"{device} - Downloading Comtrade files");

                    DownloadComtradeFiles(iedConnection, device, filteredDownloadableFileList);

                    Logger.Info($"{device} - Reading files");
                    
                    //Using the recently donwloaded files:
                    //(Each temporary folder is unique for each IED)
                    var temporaryFolder = PathHelper.GetTemporaryDownloadFolder(device);
                    var temporaryComtradeFiles = ComtradeHelper.ParseComtradeFiles(device, temporaryFolder);

                    Logger.Info($"{device} - Saving files to the DB");

                    DatabaseService.StoreComtradeFilesToDatabase(device, temporaryComtradeFiles);

                    Logger.Info($"{device} - Exporting files");

                    ExportService.ExportDisturbanceRecordings(device, temporaryComtradeFiles);

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
