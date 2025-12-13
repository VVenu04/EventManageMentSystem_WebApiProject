using Domain.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Admin 
    {
        [Key]
        public Guid AdminID { get; set; }
        public string AdminName { get; set; }=string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public decimal AdminCashBack { get; set; }
    }
}
