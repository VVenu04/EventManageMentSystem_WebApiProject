using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class CreateBookingDto
    {
        public DateTime EventDate { get; set; }
        public DateTime EventTime { get; set; }
        public string Location { get; set; }
        public List<Guid> ServiceIDs { get; set; }
        public Guid? PackageID { get; set; }
       
    }
}
