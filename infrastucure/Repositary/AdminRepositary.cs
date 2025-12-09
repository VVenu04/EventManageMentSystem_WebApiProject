using Application.DTOs.Admin;
using Application.Interface.IRepo;
using Domain.Entities;
using Google.GenAI.Types;
using infrastructure.GenericRepositary;
using infrastucure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastucure.Repositary
{
    public class AdminRepositary :GenericRepo<Admin>, IAdminRepo
    {
        private readonly ApplicationDbContext _context;

        public AdminRepositary(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task UpdateAsync(Admin admin)
        {
            _dbContext.Admins.Update(admin);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<AdminDashboardDto> GetDashboardStatsAsync()
        {
            var stats = new AdminDashboardDto();
            stats.TotalVendors = await _context.Vendors.CountAsync();
            stats.TotalCustomers = await _context.Customers.CountAsync();
            stats.TotalBookings = await _context.Bookings.CountAsync();

            // Revenue Calculation (Safe check for null)
            stats.TotalRevenue = await _context.Bookings
                .Where(b => b.BookingStatus == "Paid" || b.BookingStatus == "Completed")
                .SumAsync(b => b.TotalPrice);

            stats.RecentActivities = new List<DashboardActivityDto>(); // Placeholder for now
            return stats;
        }

        public async Task<SystemSettings> GetSystemSettingsAsync()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new SystemSettings
                {
                    SiteName = "SmartFunction",
                    SupportEmail = "support@smart.com",
                    SupportPhone = "+94 77 123 4567",
                    OfficeAddress = "Jaffna, Sri Lanka",
                    ServiceCommission = 10,
                    PackageCommission = 5,
                    CustomerCashback = 2,
                    MaintenanceMode = false
                };
                _context.SystemSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }

        public async Task UpdateSystemSettingsAsync(SystemSettings settings)
        {
            _context.SystemSettings.Update(settings);
            await _context.SaveChangesAsync();
        }

        // 🚨 FIX: Implementing Missing Methods (Error Fix)
        public async Task<Admin> GetAdminByIdAsync(Guid adminId)
        {
            return await _context.Admins.FindAsync(adminId);
        }

        public async Task UpdateAdminAsync(Admin admin)
        {
            // Force EF Core to mark as modified
            _context.Entry(admin).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }


    }
}
