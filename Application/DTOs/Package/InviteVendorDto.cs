using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Package
{
    public class InviteVendorDto
    {
        public Guid PackageID { get; set; }
        public Guid VendorIDToInvite { get; set; }
    }
}
