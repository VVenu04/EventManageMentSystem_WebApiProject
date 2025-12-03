using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ServiceItem
{
    public class ServiceSearchDto
    {
        public string? SearchTerm { get; set; }= string.Empty;
        public Guid? EventID { get; set; }

        public Guid? CategoryID { get; set; }   // (குறிப்பிட்ட Category மட்டும்)
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Location { get; set; }= string.Empty;
        public DateTime? EventDate { get; set; }
    }
}
