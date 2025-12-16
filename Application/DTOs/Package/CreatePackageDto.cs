using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Package
{
    public class CreatePackageDto
    {
        public string Name { get; set; }

        public string Description { get; set; }
        public decimal TotalPrice { get; set; } 

       
        public List<Guid> ServiceItemIDs { get; set; }
    }
}
