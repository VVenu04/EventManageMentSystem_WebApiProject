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

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
       //public string Salt { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? RegisterNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public ICollection<ServiceItem> Services { get; set; } = new List<ServiceItem>();
        public ICollection<Package> Packages { get; set; } = new List<Package>();
        public ICollection<Message> Message { get; set; } = new List<Message>();
        public int EventPerDayLimit { get; set; }
        public int TimeLimit { get; set; }
        public string? Logo { get; set; }
    }
}
