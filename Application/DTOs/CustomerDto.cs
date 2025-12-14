using Application.Attribute;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CustomerDto
    {
        public Guid CustomerID { get; set; }
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [CustomEmail]
        public string Email { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [CustomPassword]
        public string Password { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public string ProfilePhoto { get; set; }
        public string GoogleId { get; set; } = default!;
        public decimal WalletBalance { get; set; }
         

    }
}
