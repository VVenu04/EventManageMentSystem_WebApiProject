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
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context; 

        public AuthRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Customer Repo ---
        public async Task<bool> CustomerEmailExistsAsync(string email)
        {
            return await _context.Customers.AnyAsync(x => x.Email == email);
        }

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await _context.Customers.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<Customer?> AddCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        // --- Vendor Repo ---
        public async Task<bool> VendorEmailExistsAsync(string email)
        {
            return await _context.Vendors.AnyAsync(x => x.Email == email);
        }

        public async Task<Vendor?> GetVendorByEmailAsync(string email)
        {
            return await _context.Vendors.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<Vendor?> AddVendorAsync(Vendor vendor)
        {
            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();
            return vendor;
        }

        // --- Admin Repo ---
        public async Task<Admin?> GetAdminByEmailAsync(string email)
        {
            return await _context.Admins.FirstOrDefaultAsync(x => x.AdminEmail == email);
        }
        public async Task<Customer?> GetCustomerByIdAsync(Guid customerId)
        {
            return await _context.Customers.FindAsync(customerId);
        }
    }
}
