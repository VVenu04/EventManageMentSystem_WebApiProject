using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Package
{
    public class PackageRequestDto
    {

        public Guid RequestID { get; set; }
        public Guid PackageID { get; set; }
        public string PackageName { get; set; }

        public Guid SenderVendorID { get; set; }
        public string SenderName { get; set; }
        public string SenderLogo { get; set; }

        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
