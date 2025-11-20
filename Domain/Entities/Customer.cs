using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Customer
    {
        [Key]
        public Guid CustomerID { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public string? ProfilePhoto { get; set; }

        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Message> Messages { get; set; }
        public decimal WalletBalance { get; set; } = 0;
    }
}
