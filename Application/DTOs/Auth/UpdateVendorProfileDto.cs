using Application.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    public class UpdateVendorProfileDto
    {
        public string CompanyName { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; } 
        public decimal EventPerDayLimit { get; set; }
        [CustomEmail]
        public string Email { get; set; }
    }
}
