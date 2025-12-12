using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Vendor
    {
        [Key]
        public Guid VendorID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string CompanyName { get; set; }
        public string RegisterNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public ICollection<ServiceItem> Services { get; set; }
        public ICollection<Package> Packages { get; set; }
        public ICollection<Message> Message { get; set; }
        public int EventPerDayLimit { get; set; }
        public int TimeLimit { get; set; }
        public decimal VendorCashBack {  get; set; }
        public string GoogleId { get; set; }
        public string ProfilePhoto { get; set; }
        public string Logo { get; set; }
        public string Description { get; set; }
        public string? PasswordResetOtp { get; set; }
        public DateTime? PasswordResetOtpExpiry { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationToken { get; set; }
        public DateTime? TokenExpires { get; set; }

    }
}
