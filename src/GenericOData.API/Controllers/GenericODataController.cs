using GenericODataAPI.ControllerFactory;
using GenericOData.Core.Services.Models;
using GenericOData.Core.Services.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using GenericOData.Core.Services.Extensions;

namespace GenericODataAPI.Controllers
{
    [ApiController]
    [GenericODataControllerNameConvention]
    [Route("api/[version]/[dataBase]/[entity]")]
    public class GenericOdataController<T> : ControllerBase where T : class
    {
        private ILogger Logger { get; }

        private IDataService<T> _dataService { get; }

        public GenericOdataController(ILoggerFactory loggerFactory, IDataService<T> dataService)
        {
            loggerFactory.Required(nameof(loggerFactory));
            dataService.Required(nameof(dataService));
            this.Logger = loggerFactory.CreateLogger(nameof(GenericOdataController<T>));
            _dataService = dataService;
        }

        /// <summary>
        /// The Odata API endpoint for the entity.
        /// </summary>
        /// <param name="oDataQueryOptions"> The odata Specification</param>
        /// <returns> ActionResult </returns>
        [HttpGet]
        [EnableQuery]
        public async Task<ActionResult> Get(ODataQueryOptions<T> oDataQueryOptions, 
            [FromQuery]PaginationRequest paginationRequest)
        {
            var response = await _dataService.QueryAsync(RouteData, oDataQueryOptions, 
                this.Request.Headers["Authorization"], 
                paginationRequest);
            return Ok(response);
        }
    }
}