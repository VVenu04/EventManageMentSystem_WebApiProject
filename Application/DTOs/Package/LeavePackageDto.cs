using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Package
{
    public class LeavePackageDto
    {
        public Guid PackageID { get; set; }
        public Guid VendorIDToLeave { get; set; }
    }
}
