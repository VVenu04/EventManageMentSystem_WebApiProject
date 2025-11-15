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
            customer.PasswordHash = customerdto.PasswordHash;
            return customer;    
        }

        public static CustomerDto MapToCustomerDto(Customer customer)
        {
            CustomerDto customerDto = new CustomerDto();
            customerDto.Name = customer.Name;
            customerDto.Email = customer.Email;
            customerDto.PhoneNumber = customer.PhoneNumber;
            customerDto.ProfilePhoto = customer.ProfilePhoto;
            customerDto.Location = customer.Location;
            customerDto.PasswordHash = customer.PasswordHash;
            return customerDto;
        }

    }
}
