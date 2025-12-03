using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapper
{
    public class CustomerMapper
    {
        public static Customer MapToCustomer(CustomerDto customerdto)
        {
            Customer customer = new Customer();
            customer.Name = customerdto.Name;
            customer.Email = customerdto.Email;
            customer.PhoneNumber = customerdto.PhoneNumber;
            customer.ProfilePhoto = customerdto.ProfilePhoto;
            customer.Location = customerdto.Location;
            customer.GoogleId = customerdto.GoogleId;
            //customer.PasswordHash = customerdto.PasswordHash;   --> // We do NOT map the password here. The SERVICE will handle hashing process
            return customer;    
        }

        public static CustomerDto MapToCustomerDto(Customer customer)
        {
            CustomerDto customerDto = new CustomerDto();
            customerDto.CustomerID = customer.CustomerID;
            customerDto.Name = customer.Name;
            customerDto.Email = customer.Email;
            customerDto.PhoneNumber = customer.PhoneNumber;
            customerDto.ProfilePhoto = customer.ProfilePhoto;
            customerDto.Location = customer.Location;
            customerDto.GoogleId = customer.GoogleId;
            // customerDto.PasswordHash = customer.PasswordHash;
            customerDto.Password = null; // <-- Send null back
            return customerDto;
        }

        // For mapping the list in GetAllAsync()
        public static IEnumerable<CustomerDto> MapToCustomerDtoList(IEnumerable<Customer> customers)
        {
            return customers.Select(c => MapToCustomerDto(c));
        }
    }
}
