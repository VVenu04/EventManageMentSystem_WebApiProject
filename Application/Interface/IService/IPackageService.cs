using Application.DTOs.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface IPackageService
    {
        // Vendor-ஆல் ஒரு Package-ஐ உருவாக்க (Token-இல் இருந்து vendorId வரும்)
        Task<PackageDto> CreatePackageAsync(CreatePackageDto dto, Guid vendorId);

        // ஒரு Package-ஐப் பார்க்க
        Task<PackageDto> GetPackageByIdAsync(Guid packageId);

        // ஒரு Vendor-இன் எல்லா Packages-ஐயும் பார்க்க
        Task<IEnumerable<PackageDto>> GetPackagesByVendorIdAsync(Guid vendorId);
    }
}
