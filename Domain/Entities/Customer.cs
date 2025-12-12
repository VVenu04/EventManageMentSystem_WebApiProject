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

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Location { get; set; }
        public string GoogleId { get; set; }
        public string ProfilePhoto { get; set; }
         
        public ICollection<Booking> Bookings { get; set; }= new List<Booking>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public decimal WalletBalance { get; set; } = 0;
        public string? PasswordResetOtp { get; set; }

        public DateTime? PasswordResetOtpExpiry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationToken { get; set; }
        public DateTime? TokenExpires { get; set; }
    }
}
