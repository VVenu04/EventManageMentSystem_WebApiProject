using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class PackageRequest
    {
        [Key]
        public Guid RequestID { get; set; }

        public Guid PackageID { get; set; }

        // Navigation for Package
        [ForeignKey("PackageID")]
        public Package Package { get; set; }

        public Guid SenderVendorID { get; set; }

        [ForeignKey("SenderVendorID")]
        public Vendor SenderVendor { get; set; }

        public Guid ReceiverVendorID { get; set; }

        [ForeignKey("ReceiverVendorID")]
        public Vendor ReceiverVendor { get; set; }

        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
