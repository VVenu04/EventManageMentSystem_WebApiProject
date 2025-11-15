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
        public Guid CustomerID { get; set; }
        public string BookingStatus { get; set; }
        public decimal TotalPrice { get; set; }
        public string Location { get; set; }

        public DateTime EventDate { get; set; } 
        // Navigation
        public Customer? Customer { get; set; }
        public Payment? Payment { get; set; }
        public ICollection<BookingItem> BookingItems { get; set; }



    }
}
