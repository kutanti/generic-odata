using Entities;
using GenericOData.Core.Services.Models;
using GenericOData.Core.Services.Service.Interface;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Routing;

namespace GenericOData.Core.Services.Service
{
    internal class SqlService<T> : IDataService<T>
    {
        /// <summary>
        /// TODO.
        /// </summary>
        public Task<EntityBaseModel<T>> QueryAsync(RouteData routeData, ODataQueryOptions<T> oDataQueryOptions, string authToken, PaginationRequest paginationRequest)
        {
            throw new NotImplementedException();
        }
    }
}
