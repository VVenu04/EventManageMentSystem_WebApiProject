using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Service
{
    public class SimpleServiceDto
    {
        public Guid? ServiceItemID { get; set; }
        public string Name { get; set; }
        public decimal OriginalPrice { get; set; } 
    }
}
