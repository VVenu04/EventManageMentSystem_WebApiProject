using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class EventDto
    {
        public Guid EventID { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        [Display(Name = "EventName")]
        public string Name { get; set; }
    }
}
