using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace EntitySchemaGenerator.Clients
{
    public class MetadataRestClient
    {
        private readonly HttpClient _httpClient;

        private JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public MetadataRestClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T> GetSchema<T>(Uri url)
        {
            var response = await _httpClient.GetAsync(url);

            string stringContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(stringContent, options) ??
                throw new InvalidOperationException ("Unable retrieve the schema");
        }
    }
}
