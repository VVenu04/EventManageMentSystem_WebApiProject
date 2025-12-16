using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
   
    public class CompleteJobDto
    {
        public Guid BookingItemID { get; set; }
        public string Otp { get; set; } 
    }
}
