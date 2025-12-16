using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Service
{
    public class ServiceItemDto
    {
        public Guid ServiceID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; }
        public bool Active { get; set; }

        public decimal EventPerDayLimit { get; set; }
        public double TimeLimit { get; set; }

        // Relations
        public Guid VendorID { get; set; }
        public string VendorName { get; set; }
        public Guid CategoryID { get; set; }
        public string CategoryName { get; set; } 

        // --- Photos List ---
        public List<string> ImageUrls { get; set; }
        public List<string> EventNames { get; set; }
    }
}
