using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace infrastucure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingItem> BookingItems { get; set; }
        public DbSet<PackageItem> PackageItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ServiceItem> ServiceItems { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Tracking> Tracking { get; set; }
        public DbSet<PackageRequest> PackageRequests { get; set; }
        public DbSet<ServiceImage> ServiceImages { get; set; }
        public DbSet<ChatMessage>ChatMessages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ... (ஏற்கனவே உள்ள ServiceItem விதிகள்) ...

            // 🚨 FIX: PackageRequest - Vendor Relationships (Disable Cascade Delete)

            // 1. Sender Vendor
            modelBuilder.Entity<PackageRequest>()
                .HasOne(r => r.SenderVendor)
                .WithMany()
                .HasForeignKey(r => r.SenderVendorID)
                .OnDelete(DeleteBehavior.Restrict); // <-- Vendor அழிந்தால் Request அழியாது, தடுக்கும்.

            // 2. Receiver Vendor
            modelBuilder.Entity<PackageRequest>()
                .HasOne(r => r.ReceiverVendor)
                .WithMany()
                .HasForeignKey(r => r.ReceiverVendorID)
                .OnDelete(DeleteBehavior.Restrict); // <-- இதுவும் தடுக்கும்.


        }

    }

}
