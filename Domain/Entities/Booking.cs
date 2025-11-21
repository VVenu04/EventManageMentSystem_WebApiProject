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
        public string BookingStatus { get; set; }= string.Empty;
        public decimal TotalPrice { get; set; }
        public string Location { get; set; } = string.Empty;



        public DateTime EventDate { get; set; } 

        public Customer? Customer { get; set; }
        public Payment? Payment { get; set; }
        public ICollection<BookingItem> BookingItems { get; set; }=new List<BookingItem>();



    }
}
