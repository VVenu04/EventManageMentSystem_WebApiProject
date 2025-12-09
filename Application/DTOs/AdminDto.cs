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
    public class AdminDto
    {
        public Guid AdminID { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string AdminName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [CustomEmail]
        public string AdminEmail { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [CustomPassword]
        public string AdminPassword { get; set; }

        public static implicit operator AdminDto(Admin v)
        {
            throw new NotImplementedException();
        }
    }
}
