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
            return await _context.ServiceItems
                .Include(s => s.Vendor)
                .Include(s => s.Category)
                .Include(s => s.Events)        // Many-to-Many
                .Include(s => s.ServiceImages) // One-to-Many
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
                .Include(s => s.Category)      // Category பெயர் தெரிய வேண்டும்
                .Include(s => s.Events)        // Events தெரிய வேண்டும்
                .Include(s => s.ServiceImages) // படங்கள் தெரிய வேண்டும்
                                               // .Include(s => s.Vendor)     // தேவைப்பட்டால் சேர்க்கவும்

                .Where(s => s.VendorID == vendorId) // 🚨 Vendor Filter
                .OrderByDescending(s => s.ServiceItemID) // புதியது முதலில் வர
                .ToListAsync();
        }

        public async Task<ServiceItem?> GetByIdWithDetailsAsync(Guid serviceId)
        {
            return await _context.ServiceItems
                .Include(s => s.Vendor)
                .Include(s => s.Category)
                .Include(s => s.Events)
                .Include(s => s.ServiceImages)
                .FirstOrDefaultAsync(s => s.ServiceItemID == serviceId);
        }

        // 🚨 FIX: Just Remove from Context (Do NOT Save here)
        public void DeleteImages(IEnumerable<ServiceImage> images)
        {
            // இது Database-ல் இருந்து Delete Query-ஐ தயார் செய்யும்
            if (images != null && images.Any())
            {
                _context.ServiceImages.RemoveRange(images);
            }
        }

        public async Task UpdateAsync(ServiceItem service)
        {
            // EF Core ட்ராக்கிங் மூலம் மாற்றங்களைக் கண்டறிந்து சேமிக்கும்
            // .Update(service) என்று அழைக்கத் தேவையில்லை
            await _context.SaveChangesAsync();
        }


        public async Task DeleteAsync(ServiceItem service)
        {
            // 1. தொடர்புடைய படங்களை முதலில் நீக்கவும் (Optional but safe)
            // (Cascade Delete இருந்தால் இது தேவையில்லை, ஆனால் Explicit ஆக செய்வது நல்லது)
            if (service.ServiceImages != null && service.ServiceImages.Any())
            {
                _context.ServiceImages.RemoveRange(service.ServiceImages);
            }

            // 2. Service-ஐ நீக்கவும்
            _context.ServiceItems.Remove(service);

            // 3. Save Changes
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


       

    }
}
