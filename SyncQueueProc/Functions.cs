using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using ADSync.Data.Models;
using Newtonsoft.Json;
using Common;
using Microsoft.WindowsAzure.Storage.Blob;
using ADSync.Common.Models;

namespace SyncQueueProc
{
    public class Functions
    {
        public static void ProcessUserQueue([QueueTrigger("stageduser")] string queueData, IBinder binder)
        {
            try
            {
                string data = queueData;
                string containerAndBlob = "";
                BlobAttribute blobAttribute;
                if (Uri.IsWellFormedUriString(queueData, UriKind.Absolute))
                {
                    var uri = new Uri(queueData);
                    containerAndBlob = uri.AbsolutePath.Substring(1);

                    blobAttribute = new BlobAttribute(containerAndBlob, FileAccess.Read);
                    data = queueData;
                    using (var stream = binder.Bind<Stream>(blobAttribute))
                    {
                        StreamReader reader = new StreamReader(stream);
                        data = reader.ReadToEnd();
                    }
                }

                IEnumerable<StagedUser> stagedUsers = JsonConvert.DeserializeObject<IEnumerable<StagedUser>>(data);

                var res = StagedUserUtil.ProcessCollection(stagedUsers);

                if (containerAndBlob.Length > 0)
                {
                    //delete block blob
                    //Must be "FileAccess.ReadWrite"
                    blobAttribute = new BlobAttribute(containerAndBlob, FileAccess.ReadWrite);
                    var blockBlob = binder.Bind<CloudBlockBlob>(blobAttribute);
                    blockBlob.DeleteIfExists();
                }
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error processing queue 'stageduser'", System.Diagnostics.EventLogEntryType.Error, ex);

            }
        }

        public static void ProcessLogQueue([QueueTrigger("logbatch")] string queueData, IBinder binder)
        {
            try
            {
                string data = queueData;
                string containerAndBlob = "";
                BlobAttribute blobAttribute;
                if (Uri.IsWellFormedUriString(queueData, UriKind.Absolute))
                {
                    var uri = new Uri(queueData);
                    containerAndBlob = uri.AbsolutePath.Substring(1);

                    blobAttribute = new BlobAttribute(containerAndBlob, FileAccess.Read);
                    data = queueData;
                    using (var stream = binder.Bind<Stream>(blobAttribute))
                    {
                        StreamReader reader = new StreamReader(stream);
                        data = reader.ReadToEnd();
                    }
                }

                IEnumerable<SyncLogEntry> logBatch = JsonConvert.DeserializeObject<IEnumerable<SyncLogEntry>>(data);

                var res = SyncLogEntryUtil.ProcessCollection(logBatch);

                if (containerAndBlob.Length > 0)
                {
                    //delete block blob
                    //Must be "FileAccess.ReadWrite"
                    blobAttribute = new BlobAttribute(containerAndBlob, FileAccess.ReadWrite);
                    var blockBlob = binder.Bind<CloudBlockBlob>(blobAttribute);
                    blockBlob.DeleteIfExists();
                }
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error processing queue 'logbatch'", System.Diagnostics.EventLogEntryType.Error, ex);

            }
        }
    }
}
