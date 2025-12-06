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
        public double TimeLimit { get; set; } // (e.g., 3.5 நாட்கள்)

        // Relations
        public Guid CategoryID { get; set; }
        //public Guid? EventID { get; set; }

        public List<Guid> EventIDs { get; set; } = new List<Guid>();

        // --- நீங்கள் கேட்ட 5 Photos ---
        // (Frontend 5 URL-களை List-ஆக அனுப்பும்)
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
