using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PackageRequest
    {
        [Key]
        public Guid RequestID { get; set; }

        public Guid PackageID { get; set; }
        public Package Package { get; set; }

        public Guid SenderVendorID { get; set; } 
        public Guid ReceiverVendorID { get; set; } 

        public string Status { get; set; } = "Pending"; // "Pending", "Accepted", "Rejected"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
