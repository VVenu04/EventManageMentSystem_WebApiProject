using Application.Interface.IRepo;
using Domain.Entities;
using infrastucure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repositary
{
    public class PackageRequestRepository: IPackageRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public PackageRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PackageRequest> AddAsync(PackageRequest request)
        {
            _context.PackageRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<PackageRequest?> GetByIdAsync(Guid requestId)
        {
            return await _context.PackageRequests.FindAsync(requestId);
        }

        public async Task<PackageRequest?> GetRequestAsync(Guid packageId, Guid vendorId)
        {
            return await _context.PackageRequests
                .FirstOrDefaultAsync(r => r.PackageID == packageId && r.ReceiverVendorID == vendorId);
        }

        public async Task UpdateAsync(PackageRequest request)
        {
            _context.PackageRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PackageRequest>> GetPendingRequestsForVendorAsync(Guid vendorId)
        {
            return await _context.PackageRequests
                .Include(r => r.Package) // Package details தெரிய வேண்டும்
                .Where(r => r.ReceiverVendorID == vendorId && r.Status == "Pending")
                .ToListAsync();
        }

        public async Task<bool> IsVendorApprovedForPackageAsync(Guid packageId, Guid vendorId)
        {
            // Vendor அந்த Package-இன் Owner-ஆ? அல்லது Request Accept செய்தவரா?
            var isOwner = await _context.Packages.AnyAsync(p => p.PackageID == packageId && p.VendorID == vendorId);
            if (isOwner) return true;

            var isApprovedCollaborator = await _context.PackageRequests
                .AnyAsync(r => r.PackageID == packageId && r.ReceiverVendorID == vendorId && r.Status == "Accepted");

            return isApprovedCollaborator;
        }
       
        public async Task<IEnumerable<PackageRequest>> GetPendingRequestsByVendorAsync(Guid vendorId)
        {
            return await _context.PackageRequests
                .Include(r => r.Package)
                .Include(r => r.SenderVendor) // ✅ இப்போது இது பிழையின்றி வேலை செய்யும்
                .Where(r => r.ReceiverVendorID == vendorId && r.Status == "Pending")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
