using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ServiceItem
{
    public class UpdateServiceDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; }

        public decimal EventPerDayLimit { get; set; }
        public double TimeLimit { get; set; }

        public Guid CategoryID { get; set; }
        //public Guid? EventID { get; set; }
        public List<Guid> EventIDs { get; set; }
        public List<string> ImageUrls { get; set; }
    }
}
