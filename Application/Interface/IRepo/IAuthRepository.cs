using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IRepo
{
    public interface IAuthRepository
    {
        Task<bool> CustomerEmailExistsAsync(string email);
        Task<Customer?> GetCustomerByEmailAsync(string email);
        Task<Customer?> AddCustomerAsync(Customer customer);

        Task<bool> VendorEmailExistsAsync(string email);
        Task<Vendor?> GetVendorByEmailAsync(string email);
        Task<Vendor?> AddVendorAsync(Vendor vendor);

        Task<Admin?> GetAdminByEmailAsync(string email);

        Task<Customer?> GetCustomerByIdAsync(Guid customerId);
    }
}
