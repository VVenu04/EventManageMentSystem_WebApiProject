using Application.DTOs;
using Application.Interface.IRepo;
using Application.Interface.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly  ICustomerRepo _repo;
        public CustomerService(ICustomerRepo customerRepo)
        {
            _repo = customerRepo;
        }
        public Task<CustomerDto> AddCustomerAsync(CustomerDto customerDTO)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCustomerAsync(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CustomerDto> GetCustomerAsync(Guid customerId)
        {
            throw new NotImplementedException();
        }
    }
}
