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
        Task InviteVendorAsync(InviteVendorDto dto, Guid senderId);

        // 2. அழைப்பை ஏற்க/மறுக்க
        Task RespondToInvitationAsync(Guid requestId, Guid vendorId, bool isAccepted);

        // 3. Services-ஐச் சேர்க்க (பிழை வந்த இடம் இதுதான்)
        Task AddServicesToPackageAsync(AddServicesToPackageDto dto, Guid vendorId);

        // 4. Package-ஐ வெளியிட (Publish)
        Task PublishPackageAsync(Guid packageId, Guid vendorId);
    }
}
