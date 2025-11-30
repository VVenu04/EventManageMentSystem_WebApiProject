using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapper
{
    public class VendorMapper
    {
        
        public static Vendor MapToVendor(VendorDto vendorDTO)
        {
            Vendor vendor = new Vendor();
            vendor.VendorID = vendorDTO.VendorID; //  if we are updating an existing entity
            vendor.Name = vendorDTO.Name;
            vendor.Email = vendorDTO.ContactEmail; 
            vendor.PhoneNumber = vendorDTO.PhoneNumber;
            vendor.CompanyName = vendorDTO.Category; 

            // PasswordHash will be handled by the Service layer for hashing
            return vendor;
        }

        // Entity (DB) -> DTO (Output)
        public static VendorDto MapToVendorDTO(Vendor vendor)
        {
            if (vendor == null) return null;
            VendorDto vendorDTO = new VendorDto();

            vendorDTO.VendorID = vendor.VendorID; 
            vendorDTO.Name = vendor.Name;
            vendorDTO.PhoneNumber = vendor.PhoneNumber;
            vendorDTO.ContactEmail = vendor.Email; 
            vendorDTO.Category = vendor.CompanyName; 

            vendorDTO.Password = null; //  Never return the hash/password

            return vendorDTO;
        }

        // List Mapping
        public static IEnumerable<VendorDto> MapToVendorDTOList(IEnumerable<Vendor> vendors) 
        {
            return vendors.Select(v => new VendorDto
            {
                VendorID = v.VendorID,
                Name = v.Name,
                PhoneNumber = v.PhoneNumber,
                ContactEmail = v.Email,
                Category = v.CompanyName, 
                Password = null 
            }).ToList();
        }
    }
}