using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Service
    {
        [Key]
        public Guid ServiceID { get; set; }
        public string Name { get; set; } 
        public Guid VendorID { get; set; }
        public Guid CategoryID { get; set; }
        public Guid? EventID { get; set; }

        public string Photo { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal EventPerDayLimit { get; set; }
        public double TimeLimit { get; set; }
        public string Location { get; set; }
        public bool Active { get; set; }

        public Vendor? Vendor { get; set; }
        public Event? Event { get; set; }
        public Category? Category { get; set; }
        //public ICollection<Booking> Bookings { get; set; }
        public ICollection<BookingItem>? BookingItems { get; set; }
        public ICollection<PackageItem>? PackageItems { get; set; }


    }
}
