using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ServiceImage
    {
        [Key]
        public Guid ServiceImageID { get; set; }

        public string ImageUrl { get; set; } // Photo-இன் URL
        public bool IsCover { get; set; } = false; // இது main photo-வா?

        // Foreign Key
        public Guid ServiceID { get; set; }
        public Service Service { get; set; }
    }
}
