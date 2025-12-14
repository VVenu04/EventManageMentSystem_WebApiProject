using Application.Interface.IRepo;
using Domain.Entities;
using infrastructure.GenericRepositary;
using infrastucure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repositary
{
    public class CustomerRepository : GenericRepo<Customer> , ICustomerRepo
    {
        public CustomerRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<Customer> GetByEmailAsync(string email)
        {
            return await _dbContext.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }
        public async Task<Customer> GetByVerificationTokenAsync(string token)
        {
            return await _dbContext.Customers.FirstOrDefaultAsync(x => x.VerificationToken == token);
        }

        public async Task<Customer> CustomerGetByGoogleIdAsync(string googleId)
        {
            return await _dbContext.Customers.FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<Customer> GetByIdAsync(Guid id)
        {
            return await _dbContext.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.CustomerID == id);
        }

        

        public async Task UpdateAsync(Customer customer)
        {
            _dbContext.Customers.Update(customer);
            await _dbContext.SaveChangesAsync();
        }
    }
}
