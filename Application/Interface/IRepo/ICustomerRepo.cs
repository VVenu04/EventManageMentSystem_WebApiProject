using Application.Interface.IGenericRepo;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IRepo
{
    public interface ICustomerRepo : IGenericRepo<Customer>
    {
        Task UpdateAsync(Customer customer);
        Task<Customer> GetByEmailAsync(string email);
        Task<Customer> GetByIdAsync(Guid id);
        Task<Customer> CustomerGetByGoogleIdAsync(string googleId);
        Task<Customer> GetByVerificationTokenAsync(string token);
    }
}
