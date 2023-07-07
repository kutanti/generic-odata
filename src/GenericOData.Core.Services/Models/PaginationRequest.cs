using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericOData.Core.Services.Models
{
    public class PaginationRequest
    {
        public bool pagination { get; set; } = false;
        public string offset { get; set; } = string.Empty;
        public string limit { get; set; } = string.Empty;
    }
}
