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
        Task<PackageDto> CreatePackageAsync(CreatePackageDto dto, Guid vendorId);

        Task<PackageDto> GetPackageByIdAsync(Guid packageId);

        Task<IEnumerable<PackageDto>> GetPackagesByVendorIdAsync(Guid vendorId);
        Task InviteVendorAsync(InviteVendorDto dto, Guid senderId);

        Task RespondToInvitationAsync(Guid requestId, Guid vendorId, bool isAccepted);

        Task AddServicesToPackageAsync(AddServicesToPackageDto dto, Guid vendorId);

        Task PublishPackageAsync(Guid packageId, Guid vendorId);
        Task<IEnumerable<PackageDto>> GetAllPackagesAsync();
        Task<IEnumerable<PackageRequestDto>> GetPendingRequestsAsync(Guid vendorId);
        Task<PackageDto> GetPackagePreviewForCollabAsync(Guid packageId, Guid requestingVendorId);

        Task DeletePackageAsync(Guid packageId, Guid vendorId);
    }
}
