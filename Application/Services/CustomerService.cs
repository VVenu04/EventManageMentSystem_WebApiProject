using Application.DTOs;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepo _repo;
        public CustomerService(ICustomerRepo customerRepo)
        {
            _repo = customerRepo;
        }

        public async Task<CustomerDto> AddCustomerAsync(CustomerDto customerDTO)
        {
            var customer = CustomerMapper.MapToCustomer(customerDTO);

            
            // Hash the password, like in AdminService and AuthService
            if (!string.IsNullOrEmpty(customerDTO.Password))
            {
                customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(customerDTO.Password);
            }


            var addedCustomer = await _repo.AddAsync(customer);
            return CustomerMapper.MapToCustomerDto(addedCustomer);
        }

        public async Task DeleteCustomerAsync(Guid Id)
        {
            var customer = await _repo.GetByIdAsync(c => c.CustomerID == Id);
            if (customer != null)
            {
                await _repo.DeleteAsync(customer);
            }
 
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await _repo.GetAllAsync();
            return CustomerMapper.MapToCustomerDtoList(customers);
        }

        public async Task<CustomerDto> GetCustomerAsync(Guid customerId)
        {
            var customer = await _repo.GetByIdAsync(c => c.CustomerID == customerId);
            if (customer == null)
            {
                return null;
            }
            return CustomerMapper.MapToCustomerDto(customer);
        }
    }
}