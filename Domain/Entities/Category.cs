using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Category
    {
        [Key]
        public Guid CategoryID { get; set; }
        public string CategoryName { get; set; }

        // Navigation
        public ICollection<Service>? Services { get; set; }
        public ICollection<Event>? Events { get; set; }
    }
}
