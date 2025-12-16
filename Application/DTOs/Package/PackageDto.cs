using Application.DTOs.Service;
using Application.DTOs.ServiceItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Package
{
    public class PackageDto
    {
        public Guid PackageID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal TotalPrice { get; set; } 
        public bool Active { get; set; }

        public Guid VendorID { get; set; }
        public string VendorName { get; set; }

        public List<string> PackageImages { get; set; }

        public List<SimpleServiceDto> ServicesInPackage { get; set; }
    }
}
