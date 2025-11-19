using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AdminReport
    {
        public Guid ReportID { get; set; }
        public string? ReportName { get; set; }
    }
}
