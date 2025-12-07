using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IRepo
{
    public  interface IPackageRequestRepository
    {
        Task<PackageRequest> AddAsync(PackageRequest request);
        Task<PackageRequest> GetByIdAsync(Guid requestId);
        Task<PackageRequest> GetRequestAsync(Guid packageId, Guid vendorId);
        Task UpdateAsync(PackageRequest request);

        Task<IEnumerable<PackageRequest>> GetPendingRequestsForVendorAsync(Guid vendorId);

        Task<bool> IsVendorApprovedForPackageAsync(Guid packageId, Guid vendorId);
        Task<IEnumerable<PackageRequest>> GetPendingRequestsByVendorAsync(Guid vendorId);

        // Added this NEW method specifically for checking a Package's status
        Task<IEnumerable<PackageRequest>> GetPendingRequestsByPackageAsync(Guid packageId);
    }
}
