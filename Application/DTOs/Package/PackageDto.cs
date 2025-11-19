using Application.DTOs.Service;
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
        public string? Name { get; set; }
        public decimal TotalPrice { get; set; } // (Package-இன் தள்ளுபடி விலை)
        public bool Active { get; set; }

        public Guid VendorID { get; set; }
        public string? VendorName { get; set; }

        // ஒரு Package-க்குள் என்னென்ன Services உள்ளன
        public List<SimpleServiceDto>? ServicesInPackage { get; set; }
    }
}
