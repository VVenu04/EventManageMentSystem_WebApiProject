using Application.Interface.IRepo;
using Domain.Entities;
using infrastructure.GenericRepositary;
using infrastucure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repositary
{
    public class VendorRepository : GenericRepo<Vendor>, IVendorRepo
    {
        public VendorRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<Vendor> GetByEmailAsync(string email)
        {
            return await _dbContext.Vendors
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }
        public async Task<Vendor> GetByVerificationTokenAsync(string token)
        {
            return await _dbContext.Vendors.FirstOrDefaultAsync(x => x.VerificationToken == token);
        }
        public async Task<Vendor> VendorGetByGoogleIdAsync(string googleId)
        {
            return await _dbContext.Vendors.FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task UpdateAsync(Vendor vendor)
        { 
             _dbContext.Vendors.Update(vendor);
            await _dbContext.SaveChangesAsync();
        }
    }
}
