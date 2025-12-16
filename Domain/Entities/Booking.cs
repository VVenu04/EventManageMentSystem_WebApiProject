using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Booking
    {
        [Key]
        public Guid BookingID { get; set; }
        public Guid CustomerID { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime EventDate { get; set; }
        public string EventTime { get; set; }
        public string Discription { get; set; } 

        // Navigation Properties
        public Customer? Customer { get; set; }
        public Payment? Payment { get; set; }

        public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    }
}