using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities
{
    public class EntityBaseModel<T>
    {
        public int? current_page { get; set; }
        public int? current_page_size { get; set; }
        public string first_page { get; set; }
        public string next_page { get; set; }

        public string previous_page { get; set; }

        public IEnumerable<T> data { get; set; }
    }
}
