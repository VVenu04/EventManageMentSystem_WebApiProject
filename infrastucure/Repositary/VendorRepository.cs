using Application.Interface.IRepo;
using Domain.Entities;
using infrastucure.Data;
using infrastucure.GenericRepositary;
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

        public async Task UpdateAsync(Vendor vendor)
        { 
             _dbContext.Vendors.Update(vendor);
            await _dbContext.SaveChangesAsync();
        }
    }
}
