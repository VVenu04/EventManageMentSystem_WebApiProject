using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Cart
{
    public class AddToCartDto
    {
        public Guid CustomerID { get; set; }

        // Service-ஐச் சேர்க்க
        public Guid? ServiceID { get; set; }

        // அல்லது Package-ஐச் சேர்க்க
        public Guid? PackageID { get; set; }

        public DateTime EventDate { get; set; }
        public string Location { get; set; }
    }
}
