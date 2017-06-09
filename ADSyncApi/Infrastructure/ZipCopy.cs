using ADSync.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Xml;

namespace ADSyncApi.Infrastructure
{
    public static class ZipCopy
    {
        private static string _zipName = "SyncSiteSetup";

        public static void InitZip(string dirPath)
        {
            var rootPath = Path.Combine(dirPath, "Files");
            var folderPath = Path.Combine(rootPath, _zipName);

            using (var fileStream = new FileStream(Path.Combine(rootPath, _zipName + ".zip"), FileMode.Create))
            {
                var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, false);
                try
                {
                    AddFilesAndFolders(folderPath, ref archive);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    archive.Dispose();
                }
            }
        }

        public static MemoryStream SetupZip(string dirPath, RemoteSite site, string apiUrl)
        {
            string apiKey = site.ApiKey;
            var configFile = GetConfig(Path.Combine(dirPath, _zipName), apiKey, apiUrl);

            var scriptFolderName = site.SiteType.ToString();
            var scriptFolderPath = Path.Combine(dirPath, scriptFolderName);

            using (var zipFile = File.Open(Path.Combine(dirPath, _zipName + ".zip"), FileMode.Open))
            {
                Stream zipCopy = new MemoryStream();
                zipFile.CopyTo(zipCopy);

                var archive = new ZipArchive(zipCopy, ZipArchiveMode.Update, true);

                try
                {
                    //setup app config file
                    var item = archive.CreateEntry("api.config", CompressionLevel.Fastest);

                    using (var streamWriter = new StreamWriter(item.Open()))
                    {
                        streamWriter.Write(configFile);
                    }

                    //add correct PS script folder
                    AddFilesAndFolders(scriptFolderPath, ref archive, "Scripts");

                    return (zipCopy as MemoryStream);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    archive.Dispose();
                }
            }
        }
        private static void AddFilesAndFolders(string folderPath, ref ZipArchive archive, string folderRoot = "")
        {
            var fileList = Directory.EnumerateFiles(folderPath);
            AddFiles(fileList, ref archive, folderRoot);

            var folderList = Directory.EnumerateDirectories(folderPath);
            foreach (var dir in folderList)
            {
                AddFilesAndFolders(dir, ref archive, Path.Combine(folderRoot, Path.GetFileNameWithoutExtension(dir)));
            }
        }

        private static void AddFiles(IEnumerable<string> fileList, ref ZipArchive archive, string folderPath = "")
        {
            //add files in root
            foreach (var file in fileList)
            {
                var fileName = Path.GetFileName(file);
                if (fileName == "api.config.txt") continue;

                using (var stream = File.Open(file, FileMode.Open))
                {
                    var item = archive.CreateEntry(Path.Combine(folderPath, fileName), CompressionLevel.Fastest);
                    using (var itemStream = item.Open())
                    {
                        stream.CopyTo(itemStream);
                    }
                }
            }
        }

        private static string GetConfig(string dirPath, string apiKey, string apiUrl)
        {
            var file = new XmlDocument();
            file.Load(Path.Combine(dirPath, "api.config.txt"));

            var key = file.SelectSingleNode("//add[@key='ApiKey']");
            key.Attributes["value"].Value = apiKey;
            key = file.SelectSingleNode("//add[@key='SiteUrl']");
            key.Attributes["value"].Value = apiUrl;
            return file.OuterXml;
        }
    }
}