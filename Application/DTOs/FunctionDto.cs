using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class FunctionDto
    {
        public Guid VendorID { get; set; }
        public Guid CategoryID { get; set; }
        public Guid? EventID { get; set; }

        public string Photo { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public string Location { get; set; }
    }
}
