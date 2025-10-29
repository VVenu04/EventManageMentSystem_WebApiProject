using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Booking
    {
        [Key]
        public Guid BookingID { get; set; }
        public Guid ServiceID { get; set; }
        public Guid CustomerID { get; set; }

        public Guid? PackageID { get; set; }   // Optional: booking may be based on a package

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public Guid? PaymentID { get; set; }

        // Navigation
        public Service? Service { get; set; }
        public Customer? Customer { get; set; }
        public Payment? Payment { get; set; }
        public Tracking? Tracking { get; set; }

        // Link to package
        public Package? Package { get; set; }

    }
}
