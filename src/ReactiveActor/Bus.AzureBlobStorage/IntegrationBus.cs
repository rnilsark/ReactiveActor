using System;
using System.IO;
using System.Threading.Tasks;
using Bus.Abstractions;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Bus.AzureBlobStorage
{
    public class IntegrationBus : IIntegrationBus
    {
        private readonly CloudStorageAccount _storageAccount;

        private IntegrationBus(CloudStorageAccount storageAccount)
        {
            _storageAccount = storageAccount;
        }

        public static IntegrationBus Connect(string blobStorageConnectionString)
        {
            if (CloudStorageAccount.TryParse(blobStorageConnectionString, out var storageAccount))
                return new IntegrationBus(storageAccount);
            
            throw new ArgumentException($"Not a valid connection string {blobStorageConnectionString}.");
        }

        public async Task Publish<T>(T message) where T : class, IEvent
        {
            var cloudBlobClient = _storageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("events");
            await cloudBlobContainer.CreateIfNotExistsAsync();

            var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(message.MessageId.ToString());
            cloudBlockBlob.Properties.ContentType = "application/json";

            using (var ms = new MemoryStream())
            {
                var json = JsonConvert.SerializeObject(message);
                var writer = new StreamWriter(ms);
                writer.Write(json);
                writer.Flush();
                ms.Position = 0;

                await cloudBlockBlob.UploadFromStreamAsync(ms);
            }
        }
    }
}