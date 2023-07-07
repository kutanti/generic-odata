using Entities;
using GenericOData.Core.Services.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Routing;

namespace GenericOData.Core.Services.Service.Interface
{
    public interface IDataService<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeData"></param>
        /// <param name="oDataQueryOptions"></param>
        /// <param name="authToken"></param>
        /// <param name="paginationRequest"></param>
        /// <returns></returns>
        Task<EntityBaseModel<T>> QueryAsync(
            RouteData routeData, 
            ODataQueryOptions<T> oDataQueryOptions, 
            string authToken,
            PaginationRequest paginationRequest);
    }
}
