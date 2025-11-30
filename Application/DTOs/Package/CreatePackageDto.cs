using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Package
{
    public class CreatePackageDto
    {
        public string Name { get; set; }
        public decimal TotalPrice { get; set; } // (Package-இன் புதிய விலை)

        // இந்த Package-இல் சேர்க்க விரும்பும் Service-களின் ID-க்கள்
        public List<Guid> ServiceItemIDs { get; set; }
    }
}
