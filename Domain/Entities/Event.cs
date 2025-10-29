using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Event
    {
        [Key]
        public Guid EventID { get; set; }

        public string EventName { get; set; }

        public ICollection<Service> Services { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<Package> Packages { get; set; }
        public ICollection<Vendor> Vendors { get; set; }
    }
}
