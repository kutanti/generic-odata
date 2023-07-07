using Azure.Storage.Blobs;
using EntitySchemaGenerator.Options;
using GenericOData.Core.Services.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EntitySchemaGenerator.Service
{
    public class AzureBlobService
    {
        private readonly StorageConfigOptions storageConfig;
        private readonly ILogger<AzureBlobService> _logger;
        public AzureBlobService(IOptions<StorageConfigOptions> storageConfigOptions, ILogger<AzureBlobService> logger)
        {
            storageConfigOptions.Value.Required(nameof(storageConfig));
            logger.Required(nameof(logger));

            storageConfig = storageConfigOptions.Value;
            _logger = logger;
        }

        public async Task<IEnumerable<EntityMetaDataOptions>> GetMetadataFromBlobs()
        {
            string azureStorageConnection = Environment.GetEnvironmentVariable("AzureStorageConnectionString") ?? string.Empty;
            azureStorageConnection.Required("The pipeline variable 'AzureStorageConnectionString' is null or empty");
            BlobServiceClient blobServiceClient = new BlobServiceClient(azureStorageConnection);
            var containerClient = blobServiceClient.GetBlobContainerClient(storageConfig.ContainerName);
            var blobs = containerClient.GetBlobs();

            List< EntityMetaDataOptions> results = new List<EntityMetaDataOptions>();

            foreach (var blob in blobs)
            {
                var blobClient = containerClient.GetBlobClient(blob.Name);
                using var stream = new MemoryStream();
                await blobClient.DownloadToAsync(stream);
                stream.Position = 0;
                using var streamReader = new StreamReader(stream);
                var stringResult = await streamReader.ReadToEndAsync();

                var datasetMetadata = JsonConvert.DeserializeObject<EntityMetaDataOptions>(stringResult);

                if(datasetMetadata != null)
                {
                    results.Add(datasetMetadata);
                }
            }

            return results;
        }
    }
}
