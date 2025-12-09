using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CategoryDto
    {
        public Guid CategoryID { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        [Display(Name = "CategoryName")]
        public string Name { get; set; }
    }
}
