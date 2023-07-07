using System;
using System.Collections.Generic;
using System.Text;

namespace EntitySchemaGenerator.Options
{
    public record ApiMetadataOptions(Uri BaseUri, List<EntityMetaDataOptions> DataSetCollection);
}
