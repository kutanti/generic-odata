using Microsoft.AspNetCore.OData.Query;
using System.Collections.Generic;

namespace GenericOData.Core.Services.Helper.Interface
{
    public interface IODataQueryConverter
    {
        (int, string) ExtractFilterAsQueryString<T>(ODataQueryOptions<T> oDataQueryOptions);
    }
}
