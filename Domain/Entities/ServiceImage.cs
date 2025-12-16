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

        public string ImageUrl { get; set; } 
        public bool IsCover { get; set; } = false; 

        // Foreign Key
        public Guid ServiceItemID { get; set; }
        public ServiceItem Service { get; set; }
    }
}
