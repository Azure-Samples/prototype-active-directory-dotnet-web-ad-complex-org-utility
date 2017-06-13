using ADSync.Common.Enums;
using ADSync.Common.Models;
using ADSync.Data.Models;
using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace ADSyncApi.Infrastructure
{
    public static class ZipCopy
    {
        private static string _zipName = "SyncSiteSetup";

        public static async Task<MemoryStream> GetSetupZip(string siteId, string pathToFiles, string apiUrl)
        {
            var site = await RemoteSiteUtil.GetSite(siteId);
            return SetupZip(pathToFiles, site, apiUrl);
        }

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

        public static string GetCurrSiteVersion(string dirPath)
        {
            var scriptVersionFile = File.ReadAllText(Path.Combine(dirPath, "ScriptVersion.txt"));
            return scriptVersionFile;
        }

        public static MemoryStream SetupZip(string dirPath, RemoteSite site, string apiUrl)
        {
            string apiKey = site.ApiKey;
            var dirPath2 = Path.Combine(dirPath, _zipName);
            var apiFilePath = Path.Combine(dirPath2, "api.config.txt");
            var serviceConfigFilePath = Path.Combine(dirPath2, "ComplexOrgSiteAgent.exe.config");

            var apiConfigFile = GetConfig(apiFilePath, site, apiUrl);
            var serviceConfigFile = GetConfig(serviceConfigFilePath, site, apiUrl);
            var jsonConfigFile = GetJsonConfig(site, apiUrl);

            var scriptVersionFile = GetCurrSiteVersion(dirPath);

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
                        streamWriter.Write(apiConfigFile);
                    }

                    //setup service config file
                    item = archive.CreateEntry("ComplexOrgSiteAgent.exe.config", CompressionLevel.Fastest);

                    using (var streamWriter = new StreamWriter(item.Open()))
                    {
                        streamWriter.Write(serviceConfigFile);
                    }

                    //add correct PS script folder
                    AddFilesAndFolders(scriptFolderPath, ref archive, "Scripts");

                    //setup script version file
                    item = archive.CreateEntry("Scripts\\ScriptVersion.txt", CompressionLevel.Fastest);

                    using (var streamWriter = new StreamWriter(item.Open()))
                    {
                        streamWriter.Write(scriptVersionFile);
                    }

                    //setup script variables file
                    item = archive.CreateEntry("Scripts\\SyncVars.json", CompressionLevel.Fastest);

                    using (var streamWriter = new StreamWriter(item.Open()))
                    {
                        streamWriter.Write(jsonConfigFile);
                    }

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
                if (fileName == "ComplexOrgSiteAgent.exe.config") continue;
                if (fileName == "SyncVars.json") continue;

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

        private static string GetConfig(string filePath, RemoteSite site, string apiUrl)
        {
            var apiKey = site.ApiKey;

            var file = new XmlDocument();
            file.Load(filePath);

            var key = file.SelectSingleNode("//add[@key='ApiKey']");
            key.Attributes["value"].Value = apiKey;
            key = file.SelectSingleNode("//add[@key='SiteUrl']");
            key.Attributes["value"].Value = apiUrl;

            if (site.SiteType == SiteTypes.MasterHQ)
            {
                key = file.SelectSingleNode("//add[@key='AADClientID']");
                key.Attributes["value"].Value = Settings.ClientId;
                key = file.SelectSingleNode("//add[@key='AADClientKey']");
                key.Attributes["value"].Value = Settings.ClientSecret;
                key = file.SelectSingleNode("//add[@key='AADTenantID']");
                key.Attributes["value"].Value = Settings.TenantId;
            }

            return file.OuterXml;
        }

        private static string GetJsonConfig(RemoteSite site, string apiUrl)
        {
            string apiKey = site.ApiKey;
            var scriptUpdates = new Dictionary<string, string>
            {
                { "ApiKey", apiKey },
                { "ApiSite", apiUrl }
            };

            if (site.SiteType == SiteTypes.MasterHQ)
            {
                //to enable GraphAPI calls for B2B invitations
                scriptUpdates.Add("AADClientID", Settings.ClientId);
                scriptUpdates.Add("AADClientKey", Settings.ClientSecret);
                scriptUpdates.Add("AADTenantID", Settings.TenantId);
            }
            var config = JsonConvert.SerializeObject(scriptUpdates);
            return config;
        }
    }
}