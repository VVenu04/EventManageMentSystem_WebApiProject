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
        [Required]
        public string Name { get; set; }
        [Required]
        public string Category { get; set; }

        [CustomEmail]
        [Required]
        public string ContactEmail { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
    }
}
