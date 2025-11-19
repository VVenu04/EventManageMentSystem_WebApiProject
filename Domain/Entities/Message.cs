using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Message
    {
        [Key]
        public Guid MessageID { get; set; }
        public Guid CustomerID { get; set; }
        public Guid VendorID { get; set; }
        public string? Data { get; set; }

        // Navigation
        public Customer? Customer { get; set; }
        public Vendor? Vendor { get; set; }
    }
}
