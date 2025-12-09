using Application.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Forgot
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [CustomEmail]
        public string Email { get; set; }
    }
}
