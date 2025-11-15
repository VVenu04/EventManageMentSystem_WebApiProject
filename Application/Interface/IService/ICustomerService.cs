using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface ICustomerService
    {
        Task<CustomerDto> AddCustomerAsync(CustomerDto customerDTO);
        Task DeleteCustomerAsync(Guid Id);
        Task<CustomerDto> GetCustomerAsync(Guid customerId);
        Task<IEnumerable<CustomerDto>> GetAllAsync();
    }
}
