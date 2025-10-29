using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Payment
    {
        [Key]
        public Guid PaymentID { get; set; }
        public Guid BookingID { get; set; }
        public string PaymentMethod { get; set; }

        // Navigation
        public Booking Booking { get; set; }
    }
}
