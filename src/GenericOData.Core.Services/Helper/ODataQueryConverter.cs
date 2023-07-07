using GenericOData.Core.Services.Helper.Interface;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Microsoft.OData.UriParser;
using System.Text;

namespace GenericOData.Core.Services.Helper
{
    public class ODataQueryConverter : IODataQueryConverter
    {
        private readonly IEdmModelBuilder _edmModelBuilder;
        private readonly ILogger<ODataQueryConverter> _logger;

        const int defaultTopLimit = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryConverter"/> class.
        /// </summary>
        /// <param name="edmModelBuilder">edmModelBuilder.</param>
        /// <param name="sqlCompiler">sqlCompiler.</param>
        public ODataQueryConverter(IEdmModelBuilder edmModelBuilder, ILogger<ODataQueryConverter> logger)
        {
            _edmModelBuilder = edmModelBuilder ?? throw new ArgumentNullException(nameof(edmModelBuilder));
            _logger = logger ?? throw new ArgumentNullException(nameof(_logger));
        }

        public (int, string) ExtractFilterAsQueryString<T>(ODataQueryOptions<T> oDataQueryOptions)
        {
            (int topLimit, string queryString) = ConvertToQueryString(oDataQueryOptions);
            return (topLimit, queryString);
        }

        /// <inheritdoc/>
        public (int, string) ConvertToQueryString<T>(ODataQueryOptions<T> oDataQueryOptions,
            bool count = false)
        {
            var expandFilter = oDataQueryOptions.SelectExpand?.RawExpand;

            if(expandFilter is null)
            {
                return (defaultTopLimit, string.Empty);
            }

            var expandString = expandFilter.Substring(0, expandFilter.Length - 1).Replace("data(", "");

            var odataQuery = expandString.Split(';', StringSplitOptions.RemoveEmptyEntries);

            var filter = odataQuery.Where(x => x.Contains("$filter")).FirstOrDefault();
            var top = odataQuery.Where(x => x.Contains("$top")).FirstOrDefault();
            
            if (string.IsNullOrEmpty(filter) && string.IsNullOrEmpty(top))
            {
                return (defaultTopLimit, String.Empty);
            }

            int topLimit = 0;
            if(!string.IsNullOrEmpty(top) && !int.TryParse(top.Replace("$top=", "").Trim(), out topLimit))
            {
                throw new Exception("Invalid odata specification.");
            }

            if (string.IsNullOrEmpty(filter))
            {
                return (topLimit, string.Empty);
            }

            return (topLimit == 0 ? defaultTopLimit : topLimit, 
                BuildQuery(typeof(T).Name,
                    new Dictionary<string, string>
                        {
                            { "filter", filter.Replace("$filter=", "") }
                        },
                    count)
                );
        }

        private string BuildQuery(
            string tableName,
            IDictionary<string, string> odataQuery,
            bool count = false)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(tableName);
            }

            var parser = InitializeODataQueryOptionParser(tableName, odataQuery);

            try
            {
                var filterClause = parser.ParseFilter();
                var topClause = parser.ParseTop();

                StringBuilder filterBuilder = new StringBuilder();

                if (filterClause != null)
                {
                    filterBuilder = filterClause.Expression.Accept(new FilterClauseBuilder(filterBuilder));
                }

                return filterBuilder.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"filter query cannot be parsed.");
                return string.Empty;
            }
        }

        private ODataQueryOptionParser InitializeODataQueryOptionParser(string tableName, IDictionary<string, string> odataQuery)
        {
            var result = _edmModelBuilder.BuildTableModel(tableName);
            var model = result.Item1;
            var entityType = result.Item2;
            var entitySet = result.Item3;
            var parser = new ODataQueryOptionParser(model, entityType, entitySet, odataQuery);
            parser.Resolver.EnableCaseInsensitive = true;
            parser.Resolver.EnableNoDollarQueryOptions = true;
            return parser;
        }
    }
}
