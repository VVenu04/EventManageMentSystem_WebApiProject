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

        public Task<Package> AddAsync(Package package)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Package>> GetPackagesByVendorAsync(Guid vendorId)
        {
            return await _context.Packages
                .Where(p => p.VendorID == vendorId)
                .Include(p => p.Vendor)
                .Include(p => p.PackageItems)
                .ThenInclude(pi => pi.Service)
                .Where(p => p.Active == true) // Active
                .ToListAsync();
        }

        public Task<Package?> GetPackageWithServicesAsync(Guid packageId)
        {
            throw new NotImplementedException();
        }
    }
}
