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

        public decimal Payment { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public Vendor? Vendor { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
