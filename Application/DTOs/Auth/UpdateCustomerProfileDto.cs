using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    public class UpdateCustomerProfileDto
    {
        public string Name { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public string ProfilePhotoUrl { get; set; }
    }
}
