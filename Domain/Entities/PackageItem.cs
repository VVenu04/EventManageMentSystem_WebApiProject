using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PackageItem
    {
        [Key]
        public Guid PackageItemID { get; set; }

        public Guid PackageID { get; set; }
        public Guid ServiceItemID { get; set; }
        public Guid ServiceID { get; set; }

        // Navigation
        public Package? Package { get; set; }
        public ServiceItem? Service { get; set; }
    }
}
