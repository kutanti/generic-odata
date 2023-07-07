using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntitySchemaGenerator.Contracts
{
    public record EntitySchema(string RawSchema, string SchemaType);
}
