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
        public Guid? EventID { get; set; }

        // புதிய Photo URLs பட்டியல்
        // (Frontend எப்போதுமே மொத்தமாக 5 URL-களையும் அனுப்ப வேண்டும்)
        public List<string> ImageUrls { get; set; }
    }
}
