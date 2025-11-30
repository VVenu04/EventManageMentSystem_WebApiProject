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
            return await _context.PackageItems.AnyAsync(pi => pi.ServiceItemID == serviceId);
        }
        public async Task<IEnumerable<ServiceItem>> SearchServicesAsync(ServiceSearchDto searchDto)
        {
            // 1. Query-ஐ உருவாக்கு (Includes உடன்)
            var query = _context.ServiceItems // (அல்லது ServiceItems)
                .Include(s => s.Vendor)
                .Include(s=>s.Event)
                .Include(s => s.Category)
                .Include(s => s.BookingItems!) // Availability Check-க்கு இது கட்டாயம்
                    .ThenInclude(bi => bi.Booking) // Booking Date-ஐப் பார்க்க இது கட்டாயம்
                .Where(s => s.Active == true)
                .AsQueryable();

            // 2. Text Search (Null Check சேர்க்கப்பட்டுள்ளது)
            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                string term = searchDto.SearchTerm.ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(term) ||
                    s.Description.ToLower().Contains(term) ||
                    // 🚨 FIX: Vendor null-ஆ என்று பார்க்க வேண்டும்
                    (s.Vendor != null && s.Vendor.Name.ToLower().Contains(term))
                );
            }
            if (searchDto.EventID.HasValue)
            {
                // பயனர் கேட்ட EventID உள்ள Services-ஐ மட்டும் காட்டு
                // (அல்லது EventID null ஆக இருந்தால், அது எல்லா Event-க்கும் பொதுவானது என்று அர்த்தம்)
                query = query.Where(s => s.EventID == searchDto.EventID.Value || s.EventID == null);
            }

            // 3. Filter by Category
            if (searchDto.CategoryID.HasValue)
            {
                query = query.Where(s => s.CategoryID == searchDto.CategoryID.Value);
            }

            // 4. Filter by Price
            if (searchDto.MinPrice.HasValue)
            {
                query = query.Where(s => s.Price >= searchDto.MinPrice.Value);
            }
            if (searchDto.MaxPrice.HasValue)
            {
                query = query.Where(s => s.Price <= searchDto.MaxPrice.Value);
            }

            // 5. Filter by Location
            if (!string.IsNullOrEmpty(searchDto.Location))
            {
                query = query.Where(s => s.Location.ToLower().Contains(searchDto.Location.ToLower()));
            }

            // 6. 🚨 Filter by Availability (முக்கியமான Null Check திருத்தம்)
            if (searchDto.EventDate.HasValue)
            {
                var searchDate = searchDto.EventDate.Value.Date;

                query = query.Where(s =>
                    // Limit 0 என்றால் Unlimited
                    s.EventPerDayLimit == 0 ||

                    // 🚨 FIX: BookingItems null-ஆ என்று பார்க்க வேண்டும்
                    (s.BookingItems != null &&
                     s.BookingItems.Count(bi =>
                        // 🚨 FIX: bi.Booking null-ஆ என்று பார்க்க வேண்டும்
                        bi.Booking != null &&
                        bi.Booking.EventDate.Date == searchDate &&
                        bi.Booking.BookingStatus != "Cancelled"
                     ) < s.EventPerDayLimit)
                );
            }

            return await query.ToListAsync();
        }

        Task<IEnumerable<ServiceItem>> IServiceItemRepository.SearchServicesAsync(ServiceSearchDto searchDto)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<ServiceItem>> GetByCategoryIdAsync(Guid categoryId)
        {
            return await _context.ServiceItems
                .Where(s => s.CategoryID == categoryId)
                .ToListAsync();
        }
    }
}
