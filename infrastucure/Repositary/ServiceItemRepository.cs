using Application.DTOs.ServiceItem;
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
                .Include(s => s.Events)
                .Include(s => s.ServiceImages)
                .FirstOrDefaultAsync(s => s.ServiceItemID == serviceId);
        }

        public async Task<IEnumerable<ServiceItem>> GetAllAsync()
        {
            return await _context.ServiceItems
                .Include(s => s.Vendor)
                .Include(s => s.Category)
                .Include(s => s.Events)
                .Include(s => s.ServiceImages)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceItem>> GetByVendorIdAsync(Guid vendorId)
        {
            return await _context.ServiceItems
                .Include(s => s.Category)
                .Include(s => s.Events)
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
            return await _context.PackageItems.AnyAsync(pi => pi.ServiceItemID == serviceId);
        }
        public async Task<IEnumerable<ServiceItem>> SearchServicesAsync(ServiceSearchDto searchDto)
        {
            // 1. Query-ஐத் தொடங்குகிறோம் (இன்னும் Database-க்கு போகவில்லை)
            var query = _context.ServiceItems
                .Include(s => s.Vendor)
                .Include(s => s.Category)
                .Include(s => s.Events)
                .Include(s => s.ServiceImages)
                .AsQueryable();

            // 2. SearchTerm இருந்தால் Filter செய்
            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                string term = searchDto.SearchTerm.ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(term) ||
                                         s.Description.ToLower().Contains(term) ||
                                         s.Location.ToLower().Contains(term));
            }

            // 3. CategoryID இருந்தால் Filter செய்
            if (searchDto.CategoryID.HasValue)
            {
                query = query.Where(s => s.CategoryID == searchDto.CategoryID);
            }
            if (searchDto.EventID.HasValue)
            {
                // புதிய Code: Events லிஸ்டில் இந்த ID இருக்கிறதா என பார்க்கிறோம்
                query = query.Where(s => s.Events.Any(e => e.EventID == searchDto.EventID.Value));
            }
            // 4. Price Range
            if (searchDto.MinPrice.HasValue)
                query = query.Where(s => s.Price >= searchDto.MinPrice);

            if (searchDto.MaxPrice.HasValue)
                query = query.Where(s => s.Price <= searchDto.MaxPrice);

            // 5. முடிவுகளை எடு (Execute Query)
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<ServiceItem>> GetByCategoryIdAsync(Guid categoryId)
        {
            return await _context.ServiceItems
                .Where(s => s.CategoryID == categoryId)
                .ToListAsync();
        }


        public async Task<ServiceItem> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.ServiceItems
                .Include(s => s.Events)        // பழைய Events தேவை
                .Include(s => s.ServiceImages) // பழைய Images தேவை
                .FirstOrDefaultAsync(s => s.ServiceItemID == id);
        }
    }
}
