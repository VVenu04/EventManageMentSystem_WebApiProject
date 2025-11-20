using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Package
{
    public class AddServicesToPackageDto
    {
        public Guid PackageID { get; set; }
        public List<Guid> ServiceIDs { get; set; }= new List<Guid>();
    }
}
