using Application.Interface.IRepo;
using Domain.Entities;
using infrastucure.Data;
using infrastucure.GenericRepositary;
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
        public AdminRepositary(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task UpdateAsync(Admin admin)
        {
            _dbContext.Admins.Update(admin);
            await _dbContext.SaveChangesAsync();
        }

    }
}
