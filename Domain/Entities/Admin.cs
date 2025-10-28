using Domain.BaseEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Admin : BaseEntity
    {
        public  string AdminName { get; set; }
        public   string AdminEmail { get; set; }
        public   string AdminPassword { get; set; }

    }
}
