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
        private static bool TestConnection(Device device)
        {
            bool isConnected = false;
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send(device.IPAddress);
                isConnected = reply.Status == IPStatus.Success;
            }
            catch (PingException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                DatabaseService.UpdateIEDConnectionStatus(device, isConnected);
            }
            return isConnected;
        }

        //private static bool TestConnection(Device device)
        //{
        //    var con = new IedConnection();
        //    var isConnected = false;
        //    try
        //    {
        //        con.Connect(device.IPAddress);
        //        con.Release();
        //        isConnected = true;
        //    }
        //    catch (IedConnectionException)
        //    { }
        //    catch (Exception e)
        //    { Console.WriteLine(e); }
        //    finally
        //    {
        //        con.Dispose();
        //        DatabaseService.UpdateIEDConnectionStatus(device, isConnected);
        //    }
        //    return isConnected;
        //}

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
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Main entry point for the IED Comtrades.
        /// </summary>
        /// <param name="device"></param>
        private static void ProcessDeviceComtradeFiles(Device device)
        {
            var iedConnection = new IedConnection();
            try
            {
                //Connect:
                iedConnection.Connect(device.IPAddress);

                //Download a list of files in the IED:
                //Uses string.Empty as main root of the IED.
                var downloadableFileList = GetDownloadableFileList(iedConnection, device, string.Empty);

                //Remove the files that have been already downloaded before:
                //It will filter the files that are not in the DB.
                var filteredDownloadableFileList = FilterExistingFiles(device, downloadableFileList);

                if (filteredDownloadableFileList.Count() > 0)
                {
                    //Download the new files:
                    DownloadComtradeFiles(iedConnection, device, filteredDownloadableFileList);

                    //Parse the downloaded files:
                    var temporaryComtradeFiles = ParseTemporaryComtradeFiles(device);

                    //Store the new files in the database:
                    StoreComtradeFilesToDatabase(device, temporaryComtradeFiles);

                    //Export the new files to the export path:
                    ExportDisturbanceRecordings(device, temporaryComtradeFiles);

                    //Remove Temporary files
                    RemoveTemporaryFiles(device);
                }
                //Close connection:
                iedConnection.Release();
            }
            catch (IedConnectionException e)
            {
                Console.WriteLine(e.GetIedClientError());
                Console.WriteLine(e.GetErrorCode());
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                //TODO: remove temporary files (?)
                iedConnection.Dispose();
                //try
                //{
                //    //Close connection:
                //    iedConnection.Close();
                //}
                //catch (Exception e)
                //{
                //    iedConnection.Dispose();
                //    Console.WriteLine(e);
                //}
            }
        }

        private static IEnumerable<(string, ulong, uint)> GetDownloadableFileList(IedConnection iedConnection, Device device, string directoryName)
        {
            //TODO: Check device is still connected? or TryCatch;

            //Get Files from Device:
            var files = iedConnection.GetFileDirectory(directoryName);

            //Init empty list:
            var downloadableFileList = new List<(string FileName, ulong CreationTime, uint FileSize)>();

            //Foreach file in the Device:
            foreach (var file in files)
            {
                var filename = file.GetFileName();

                //TODO: Add a Logger for this >>
                //List de device file:
                Console.WriteLine($"{device.Name} - {directoryName}{filename}");

                //Check if file is a directoty or a file:
                if (filename.IsDirectory())
                {
                    //If directory, recursive call with directory name:
                    downloadableFileList.AddRange(GetDownloadableFileList(iedConnection, device, directoryName + filename));
                }
                else
                {
                    //If file, and a valid download extension, add file to downloadable list:                    
                    if (filename.IsDownloadable())
                        downloadableFileList.Add((directoryName + filename, file.GetLastModified(), file.GetFileSize()));
                }
            }
            return downloadableFileList;
        }

        private static IEnumerable<(string, ulong, uint)> FilterExistingFiles(Device device, IEnumerable<(string FileName, ulong CreationTime, uint FileSize)> downloadableFileList)
        {
            //Init empty list:
            var filteredDownloadableFileList = new List<(string FileName, ulong LastModifiedDate, uint FileSize)>();

            using (var context = new SystemContext())
            {
                //Get the DB device:
                var dev = context.Devices
                                 .Include(x => x.DisturbanceRecordings)
                                    .ThenInclude(x => x.DRFiles)
                                 .FirstOrDefault(x => x.Id.Equals(device.Id));

                //If not found, return empty list:
                if (dev == null)
                    return filteredDownloadableFileList;

                var drFiles = dev.DisturbanceRecordings.SelectMany(x => x.DRFiles);

                //Foreach file in the downloadableFileList:
                foreach (var downloadableFile in downloadableFileList)
                {
                    //If the ied already has that file (should have it in the database), skip;
                    if (drFiles
                        .Any(x => x.FileName.Equals(downloadableFile.FileName.GetDestinationFilename())
                             && x.FileSize.Equals(downloadableFile.FileSize)))
                        continue;

                    //otherwise add that file to the filtered download list:
                    filteredDownloadableFileList.Add(downloadableFile);
                }
            }
            return filteredDownloadableFileList;
        }

        private static void DownloadComtradeFiles(IedConnection iedConnection, Device device, IEnumerable<(string FileName, ulong LastModifiedDate, uint FileSize)> downloadableFileList, bool deleteExistingDRs = false)
        {
            //TODO: Check device is still connected? or TryCatch;

            //For each file in the download list:
            foreach (var (FileName, _, _) in downloadableFileList)
            {
                //TODO: Check if the GetTemporaryDownloadPath works on both IEDs: 670, 615;
                //var destinationFilename = PathService.GetTemporaryDownloadPath(device, FileName.ReplaceAltDirectorySeparator().CleanFileName());
                var destinationFilename = PathService.GetTemporaryDownloadPath(device, FileName.GetDestinationFilename());

                using (var fs = new FileStream(destinationFilename, FileMode.Create, FileAccess.ReadWrite))
                using (var w = new BinaryWriter(fs))
                {
                    iedConnection.GetFile(FileName, GetFileHandler, w);
                }
            }
        }

        private static List<DisturbanceRecording> ParseTemporaryComtradeFiles(Device device)
        {
            //Init empty list:
            var disturbanceRecordings = new List<DisturbanceRecording>();

            //Using the recently donwloaded files:
            //(Each temporary folder is unique for each IED)
            var temporaryFolder = PathService.GetTemporaryDownloadFolder(device);

            /* Will parse:
             * -> Read 
             * -> Extract Trigger Date 
             * -> Extract Trigger Length 
             * -> Extract Trigger Channel
             * -> Group into a DisturbanceRecording
             */

            //Every DR Zip file that contains multiple Single files:
            disturbanceRecordings.AddRange(parseTemporaryZipFiles(temporaryFolder, device.Id));

            //And every single file:
            disturbanceRecordings.AddRange(parseTemporarySingleFiles(temporaryFolder, device.Id));

            //TODO: Test, pero en todo caso, sacar todos los que tengan un size = 0;
            //Un caso particular es que no se descarga el contenido de las DR, el archivo queda en 0;
            disturbanceRecordings.RemoveAll(x => x.DRFiles.Any(y => y.FileSize.Equals(0)));

            return disturbanceRecordings;
        }

        private static List<DisturbanceRecording> parseTemporaryZipFiles(string temporaryFolder, int deviceId)
        {
            //Init empty list;
            var disturbanceRecordings = new List<DisturbanceRecording>();

            //Get zip files Collection;
            var zipFileList = new DirectoryInfo(temporaryFolder)
                .EnumerateFiles("*.zip", SearchOption.AllDirectories);

            //Iterate over each zip file.
            //Each zip file should be a DisturbanceRecording;
            foreach (var zipFileInfo in zipFileList)
            {
                using (var zipFile = ZipFile.OpenRead(zipFileInfo.FullName))
                {
                    //Init Empty DR;
                    var dr = new DisturbanceRecording { DeviceId = deviceId };

                    //Parse DR Group:
                    var drFiles = ComtradeExtensions.ParseDRZipGroup(zipFile.Entries);

                    if (drFiles.Count > 0)
                    {
                        dr.DRFiles.AddRange(drFiles);
                        dr.Name = zipFileInfo.Name;
                        dr.TriggerTime = drFiles.FirstOrDefault().CreationTime;

                        //Add the DR to the collection:
                        disturbanceRecordings.Add(dr);
                    }

                }
            }
            return disturbanceRecordings;
        }

        private static IEnumerable<DisturbanceRecording> parseTemporarySingleFiles(string temporaryFolder, int deviceId)
        {
            //Init empty list;
            var disturbanceRecordings = new List<DisturbanceRecording>();

            //Get all non-ZIP files Collection;
            var drFileList = new DirectoryInfo(temporaryFolder)
                                    .EnumerateFiles("*.*", SearchOption.AllDirectories)
                                    .Where(x => !x.Name.ToUpper().Contains(".ZIP"));

            //Group files by their name. At least in some Tested IEDs,
            //DR Files all have the same name.
            var drFileGroups = drFileList.GroupBy(x => x.Name.GetNameWithoutExtension());

            //Iterate over each file group.
            //Each file group should be a DisturbanceRecording;
            foreach (var drFileGroup in drFileGroups)
            {
                //Init Empty DR;
                var dr = new DisturbanceRecording { DeviceId = deviceId };

                //Parse DR Group:
                var drFiles = ComtradeExtensions.ParseDRFilesGroup(drFileGroup);

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

        private static void StoreComtradeFilesToDatabase(Device device, List<DisturbanceRecording> temporaryComtradeFiles)
        {
            using (var context = new SystemContext())
            {
                //Get the DB device:
                var dev = context.Devices
                                 .Include(ied => ied.DisturbanceRecordings)
                                    .ThenInclude(dr => dr.DRFiles)
                                 .FirstOrDefault(x => x.Id.Equals(device.Id));


                foreach (var item in temporaryComtradeFiles)
                {
                    dev.DisturbanceRecordings.Add(item);
                }

                context.SaveChanges();
            }

            using (var context = new SystemContext())
            {
                //Get the DB device:
                var dev = context.Devices
                                 .Include(x => x.DisturbanceRecordings)
                                    .ThenInclude(dr => dr.DRFiles)
                                 .FirstOrDefault(x => x.Id.Equals(device.Id));
            }

        }

        private static void ExportDisturbanceRecordings(Device device, List<DisturbanceRecording> temporaryComtradeFiles)
        {
            var exportPath = PathService.GetDeviceExportFolder(device);

            foreach (var item in temporaryComtradeFiles)
            {
                var zipFilename = $"{item.TriggerTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture)},{item.TriggerTime.ToString("hhmmssfff", CultureInfo.InvariantCulture)},{device.Bay},{device.Name}.zip";
                var zipFileInfo = new FileInfo(PathService.ValidatePath(exportPath, zipFilename));

                if (zipFileInfo.Exists)
                    continue;

                using (ZipArchive zip = ZipFile.Open(zipFileInfo.FullName, ZipArchiveMode.Update))
                {
                    foreach (var drFile in item.DRFiles)
                    {
                        var zipEntry = zip.CreateEntry(drFile.FileName);
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

        private static void RemoveTemporaryFiles(Device device)
        {
            //Delete the donwloaded file folder and recursive files:
            var temporaryFolder = PathService.GetTemporaryDownloadFolder(device);
            Directory.Delete(temporaryFolder, true);
        }

        private static bool GetFileHandler(object parameter, byte[] data)
        {
            Console.WriteLine("received " + data.Length + " bytes");
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
