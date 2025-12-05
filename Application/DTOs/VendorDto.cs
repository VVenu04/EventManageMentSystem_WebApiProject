using Application.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class VendorDto
    {
        public Guid VendorID { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Category { get; set; }

        [CustomEmail]
        [Required]
        public string ContactEmail { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

        public string Password { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; } // (Photo upload செய்த பின் வரும் URL)
        public decimal EventPerDayLimit { get; set; }
        public string Email { get; set; } // (Photo upload செய்த பின் வரும் URL)




    }
}
