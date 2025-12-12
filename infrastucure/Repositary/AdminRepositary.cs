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

            // 1. Counts
            stats.TotalVendors = await _context.Vendors.CountAsync();
            stats.TotalCustomers = await _context.Customers.CountAsync();
            stats.TotalBookings = await _context.Bookings.CountAsync();
            stats.AdminCashBack = await _context.Bookings
                .Where(b => b.BookingStatus == "Completed")
                .SumAsync(b => b.TotalPrice);

            // Total Payments (Assuming you have a Payments table)
            // If table doesn't exist yet, use Bookings TotalPrice where Status='Paid'
            // For now assuming Payments table exists or sum from Bookings:
            stats.TotalRevenue = await _context.Bookings
                .Where(b => b.BookingStatus == "Paid" || b.BookingStatus == "Completed")
                .SumAsync(b => b.TotalPrice);

            // 2. Recent Activities (Logic: Get latest 3 bookings & 2 new vendors)
            var activities = new List<DashboardActivityDto>();

            // A. Get Latest Bookings
            var recentBookings = await _context.Bookings
                .Include(b => b.Customer)
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .Select(b => new DashboardActivityDto
                {
                    Title = "New Booking",
                    Description = $"{b.Customer.Name} booked a service.",
                    Time = b.CreatedAt,
                    Type = "Booking",
                    Icon = "fa-calendar-check",
                    BgColor = "bg-blue-500"
                }).ToListAsync();

            // B. Get Latest Vendors
            var newVendors = await _context.Vendors
                .OrderByDescending(v => v.CreatedAt) // Ensure Vendor has CreatedAt
                .Take(2)
                .Select(v => new DashboardActivityDto
                {
                    Title = "New Vendor Registration",
                    Description = $"{v.CompanyName} joined the platform.",
                    Time = v.CreatedAt,
                    Type = "Vendor",
                    Icon = "fa-store",
                    BgColor = "bg-orange-500"
                }).ToListAsync();

            // Merge and Sort by Time
            activities.AddRange(recentBookings);
            activities.AddRange(newVendors);

            stats.RecentActivities = activities.OrderByDescending(a => a.Time).Take(5).ToList();

            return stats;
        }

    }
}
