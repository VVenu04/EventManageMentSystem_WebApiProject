using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ServiceItem
{
    public class CreateServiceDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; }

        // Limits
        public decimal EventPerDayLimit { get; set; }
        public double TimeLimit { get; set; } 

        // Relations
        public Guid CategoryID { get; set; }
        //public Guid? EventID { get; set; }

        public List<Guid> EventIDs { get; set; } = new List<Guid>();

        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
