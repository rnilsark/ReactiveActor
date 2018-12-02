using System;
using System.Fabric;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Actor1
{
    internal class Bus
    {
        private readonly CloudStorageAccount _storageAccount;

        private Bus(CloudStorageAccount storageAccount)
        {
            _storageAccount = storageAccount;
        }

        public static Bus Connect(string connectionString)
        {
            if (CloudStorageAccount.TryParse(connectionString, out var storageAccount))
                return new Bus(storageAccount);
            
            throw new Exception($"Not a valid connection string {connectionString}.");
        }

        public async Task Publish(IntegrationEvent integrationEvent)
        {
            try
            {
                var cloudBlobClient = _storageAccount.CreateCloudBlobClient();
                var cloudBlobContainer = cloudBlobClient.GetContainerReference("events");
                if (await cloudBlobContainer.CreateIfNotExistsAsync())
                {
                    ActorEventSource.Current.Message("Created container '{0}'", cloudBlobContainer.Name);
                }

                var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(integrationEvent.EventId.ToString());
                cloudBlockBlob.Properties.ContentType = "application/json";

                using (var ms = new MemoryStream())
                {
                    var json = JsonConvert.SerializeObject(integrationEvent);
                    var writer = new StreamWriter(ms);
                    writer.Write(json);
                    writer.Flush();
                    ms.Position = 0;

                    await cloudBlockBlob.UploadFromStreamAsync(ms);
                }
            }
            catch (StorageException ex)
            {
                ActorEventSource.Current.Message("Error returned from the service: {0}", ex.Message);
            }
        }
    }
}