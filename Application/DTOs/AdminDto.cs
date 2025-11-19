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

        [Required]
        public string? AdminName { get; set; }

        [CustomEmail]
        public string? AdminEmail { get; set; }

        [Required]
        public string? AdminPassword { get; set; }

        public static implicit operator AdminDto(Admin v)
        {
            throw new NotImplementedException();
        }
    }
}
