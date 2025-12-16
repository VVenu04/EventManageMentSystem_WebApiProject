using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs.Cart
{
    public class AddToCartDto
    {
        [JsonIgnore]
        public Guid CustomerID { get; set; }

        
        public Guid? ServiceID { get; set; }

        public Guid? PackageID { get; set; }

        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public string? EventTime { get; set; }
        public string? Description { get; set; }
    }
}
