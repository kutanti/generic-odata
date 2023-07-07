using EntitySchemaGenerator.Clients;
using EntitySchemaGenerator.Contracts;
using EntitySchemaGenerator.Handlers;
using EntitySchemaGenerator.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntitySchemaGenerator.Service
{
    public class EntityGeneratorService : IEntityGeneratorService
    {
        private readonly MetadataRestClient _metadataRestClient;
        private readonly ApiMetadataOptions _apiMetadataOptions;
        private readonly AvroSchemaHandler _avroSchemaHandler;
        private readonly AzureBlobService _azureBlobService;
        private readonly ILogger<EntityGeneratorService> _logger;

        public EntityGeneratorService(MetadataRestClient metadataRestClient,
            IOptions<ApiMetadataOptions> apiMetadataOptions,
            AvroSchemaHandler avroSchemaHandler,
            AzureBlobService azureBlobService,
            ILogger<EntityGeneratorService> logger)
        {
            _metadataRestClient = metadataRestClient;
            _apiMetadataOptions = apiMetadataOptions.Value;
            _avroSchemaHandler = avroSchemaHandler;
            _azureBlobService = azureBlobService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            List<EntityMetaDataOptions> metadata = new List<EntityMetaDataOptions>();
            try
            {
                // If the metadata are in blobs.
                metadata.AddRange(await _azureBlobService.GetMetadataFromBlobs());
            }
            catch (Exception ex)
            {
                // Fall back to appsettings for loading metadata.
                _logger.LogError(ex,"Unable to load metadata from the blob");
                _logger.LogInformation("Loading metadata from configuration");
                metadata.AddRange(_apiMetadataOptions.DataSetCollection);
            }

            foreach (var item in metadata)
            {
                Uri dataSetUri = new Uri($"{_apiMetadataOptions.BaseUri}/schema");
                var response = await _metadataRestClient.GetSchema<EntitySchema>(dataSetUri);
                GenerateClass(response, item.ApiPath);
            }
        }

        private void GenerateClass(EntitySchema entitySchema, string directoryPath)
        {
            switch (entitySchema.SchemaType.ToUpperInvariant())
            {
                case "AVRO":
                    _avroSchemaHandler.GenerateClass(entitySchema.RawSchema, directoryPath);
                    break;
                default:
                    throw new InvalidOperationException("Unable to parse schema.");
            }
        }
    }
}
