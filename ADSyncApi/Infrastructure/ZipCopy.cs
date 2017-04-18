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

        public static void InitZip(string dirPath)
        {
            var folderPath = Path.Combine(dirPath, "Files");
            var rootFolder = Directory.EnumerateDirectories(folderPath);

            foreach (var dir in rootFolder)
            {
                var zipName = string.Format("{0}.zip", Path.GetFileName(dir));
                var folder = Directory.EnumerateFiles(dir);

                using (var fileStream = new FileStream(Path.Combine(folderPath, zipName), FileMode.Create))
                {
                    using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, false))
                    {
                        foreach (var file in folder)
                        {
                            var fileName = Path.GetFileName(file);

                            using (var stream = File.Open(file, FileMode.Open))
                            {
                                var item = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                                using (var itemStream = item.Open())
                                {
                                    stream.CopyTo(itemStream);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}