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
    public class ServiceItemRepository : IServiceItemRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceItem> AddAsync(ServiceItem service)
        {
            _context.ServiceItems.Add(service);
            await _context.SaveChangesAsync();
            return service;
        }

        public async Task<ServiceItem?> GetByIdAsync(Guid serviceId)
        {
            // Service-ஐ எடுக்கும்போது, related data-வையும் (Vendor, Category) எடு
            return await _context.ServiceItems
                .Include(s => s.Vendor)
                .Include(s => s.Category)
                .Include(s => s.Event)
                .Include(s => s.ServiceImages)
                .FirstOrDefaultAsync(s => s.ServiceItemID == serviceId);
        }

        public async Task<IEnumerable<ServiceItem>> GetAllAsync()
        {
            return await _context.ServiceItems
                .Include(s => s.Vendor)
                .Include(s => s.Category)
                .Include(s => s.Event)
                .Include(s => s.ServiceImages)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceItem>> GetByVendorIdAsync(Guid vendorId)
        {
            return await _context.ServiceItems
                .Include(s => s.Category)
                .Include(s => s.Event)
                .Where(s => s.VendorID == vendorId)
                .Include(s => s.ServiceImages)
                .ToListAsync();
        }

        public async Task UpdateAsync(ServiceItem service)
        {
            _context.ServiceItems.Update(service);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ServiceItem service)
        {
            _context.ServiceItems.Remove(service);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> IsServiceInAnyPackageAsync(Guid serviceId)
        {
            return await _context.PackageItems.AnyAsync(pi => pi.ServiceID == serviceId);
        }
    }
}
