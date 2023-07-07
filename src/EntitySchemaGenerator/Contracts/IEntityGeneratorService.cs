using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EntitySchemaGenerator.Contracts
{
    public interface IEntityGeneratorService
    {
        Task ExecuteAsync();
    }
}
