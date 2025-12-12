using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Package
    {
        [Key]
        public Guid PackageID { get; set; }
        public Guid VendorID { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsActive { get; set; } = false; 
        public string Status { get; set; } = "Draft";
        public Vendor Vendor { get; set; }

       // public ICollection<PackageItem> PackageItems { get; set; }
        public ICollection<PackageRequest> PackageRequests { get; set; }= new List<PackageRequest>();
        public ICollection<PackageItem> PackageItems { get; set; } = new List<PackageItem>();
    }
}

