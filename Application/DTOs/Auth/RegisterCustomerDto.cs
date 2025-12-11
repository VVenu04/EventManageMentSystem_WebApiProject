using Application.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    public class RegisterCustomerDto
    {
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [CustomEmail]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [CustomPassword]
        public string Password { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string PhoneNumber { get; set; }
    }
}
