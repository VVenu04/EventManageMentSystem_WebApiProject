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
    public class ServiceRepository: IServiceRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Service> AddAsync(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service;
        }

        public async Task<Service> GetByIdAsync(Guid serviceId)
        {
            // Service-ஐ எடுக்கும்போது, related data-வையும் (Vendor, Category) எடு
            return await _context.Services
                .Include(s => s.Vendor)
                .Include(s => s.Category)
                .Include(s => s.Event)
                .FirstOrDefaultAsync(s => s.ServiceID == serviceId);
        }

        public async Task<IEnumerable<Service>> GetAllAsync()
        {
            return await _context.Services
                .Include(s => s.Vendor)
                .Include(s => s.Category)
                .Include(s => s.Event)
                .ToListAsync();
        }

        public async Task<IEnumerable<Service>> GetByVendorIdAsync(Guid vendorId)
        {
            return await _context.Services
                .Include(s => s.Category)
                .Include(s => s.Event)
                .Where(s => s.VendorID == vendorId)
                .ToListAsync();
        }

        public async Task UpdateAsync(Service service)
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Service service)
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }
    }
}
