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

        public async Task<Package> GetPackageWithServicesAsync(Guid packageId)
        {
            return await _context.Packages
                // Package-இல் உள்ள Items-ஐ Include செய்
                .Include(p => p.PackageItems)
                // அந்த Item-உடைய Service-ஐ Include செய்
                .ThenInclude(pi => pi.Service)
                // அந்த Service-உடைய Vendor-ஐ Include செய் (Limit check-க்குத் தேவை)
                .ThenInclude(s => s.Vendor)
                .FirstOrDefaultAsync(p => p.PackageID == packageId);
        }
    }
}
