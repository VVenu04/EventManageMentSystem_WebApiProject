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
    public class PackageRepository:IPackageRepository
    {
        private readonly ApplicationDbContext _context;

        public PackageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Package> AddAsync(Package package)
        {
            await _context.Packages.AddAsync(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<IEnumerable<Package>> GetPackagesByVendorAsync(Guid vendorId)
        {
            return await _context.Packages
                .Where(p => p.VendorID == vendorId)
                .Include(p => p.Vendor)
                .Include(p => p.PackageItems)
                .ThenInclude(pi => pi.Service)
               //.Where(p => p.IsActive == true) // Active
                .ToListAsync();
        }
        public async Task UpdateAsync(Package package)
        {
            _context.Packages.Update(package);
            await _context.SaveChangesAsync();
        }

        public async Task<Package?> GetPackageWithServicesAsync(Guid packageId)
        {
            return await _context.Packages
                .Include(p => p.Vendor)
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.Service) 
                        .ThenInclude(s => s.ServiceImages) 
                .FirstOrDefaultAsync(p => p.PackageID == packageId);
        }
    }
}
