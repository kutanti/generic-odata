using GenericOData.Core.Services.Exceptions;
using GenericOData.Core.Services.Extensions;
using GenericOData.Core.Services.Helper.Interface;
using GenericOData.Core.Services.Models;
using GenericOData.Core.Services.Service.Interface;
using Entities;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace GenericOData.Core.Services.Service
{
    public class HttpService<T> : IDataService<T>
    {
        private ILogger _logger { get; set; }

        private readonly IHttpClientFactory _clientFactory;

        private readonly IODataQueryConverter _oDataQueryConverter;

        private readonly HttpClient _httpClient;

        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
        public HttpService(ILoggerFactory loggerFactory, IHttpClientFactory clientFactory, IODataQueryConverter oDataQueryConverter)
        {
            _logger = loggerFactory.CreateLogger(nameof(HttpService<T>));
            _clientFactory = clientFactory;
            _httpClient = _clientFactory.CreateClient("ApiClient");
            _oDataQueryConverter = oDataQueryConverter;
        }

        /// <summary>
        /// QueryAsync
        /// </summary>
        /// <param name="routeData"></param>
        /// <param name="oDataQueryOptions"></param>
        /// <param name="authToken"></param>
        /// <param name="paginationRequest"></param>
        /// <returns></returns>
        public async Task<EntityBaseModel<T>> QueryAsync(
            RouteData routeData,
            ODataQueryOptions<T> oDataQueryOptions,
            string authToken,
            PaginationRequest paginationRequest)
        {
            string dataSetApiPath = ConstructApiPath(routeData, oDataQueryOptions, paginationRequest);
            var response = await GetAsync(dataSetApiPath, authToken);

            try
            {
                return ProcessResponse(response);
            }
            catch (JsonException ex)
            {
                this._logger.LogError(ex, "Failed to de-serialize the API response.");
                throw;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error Occured while querying API.");
                throw;
            }

        }

        private EntityBaseModel<T> ProcessResponse(HttpResponseMessage response)
        {
            var stringContent = response.Content.ReadAsStringAsync().Result;
            
            var responseObj = JsonConvert.DeserializeObject<EntityBaseModel<T>>(stringContent, jsonSerializerSettings);

            if (responseObj is null)
            {
                string errorMessage = "The API response could not be parsed.";
                this._logger.LogError(errorMessage);
                throw new GenericODataException(errorMessage, HttpStatusCode.InternalServerError);
            }

            return responseObj;
        }

        private async Task<HttpResponseMessage> GetAsync(string dataSetApiPath, string authToken)
        {
            SetAuthToken(authToken);
            var response = await _httpClient.GetAsync(dataSetApiPath);
            // Removing, to renew the client token per request.
            // TODO: Handle concurrency race conditions.

            return response.IsSuccessStatusCode ? response
                : throw new GenericODataException("API call failed with errors.", response.StatusCode);
        }

        private string ConstructApiPath(RouteData routeData, ODataQueryOptions<T> oDataQueryOptions, PaginationRequest paginationRequest)
        {
            var businessUnit = routeData.Values.GetValueOrDefault("businessUnit") as string;
            var version = routeData.Values.GetValueOrDefault("version") as string;
            var dataBase = routeData.Values.GetValueOrDefault("dataBase") as string;

            (int topLimit, string filterQuery) = _oDataQueryConverter.ExtractFilterAsQueryString<T>(oDataQueryOptions);

            if (paginationRequest?.pagination ?? false)
            {
                paginationRequest.offset.Required("Offset query param is required");
                paginationRequest.limit.Required("Limit query param is required");

                string paginationQuery = $"pagination=true&offset={paginationRequest.offset}&limit={paginationRequest.limit}";

                return $"{businessUnit}/{version}/{dataBase}/{typeof(T).Name.ToLowerInvariant()}/?{filterQuery}&{paginationQuery}";
            }

            return $"{businessUnit}/{version}/{dataBase}/{typeof(T).Name.ToLowerInvariant()}/?{filterQuery}limit={topLimit}";
        }

        private void SetAuthToken(string authToken)
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", authToken);
        }
    }
}
