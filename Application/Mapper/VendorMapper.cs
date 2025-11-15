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
            vendor.Name = vendorDTO.Name;
            vendor.PhoneNumber = vendorDTO.PhoneNumber;
             
            return vendor;
        }

        public static VendorDto MapToVendorDTO(Vendor vendor)
        {
            VendorDto vendorDTO = new VendorDto();
            vendorDTO.Name = vendor.Name;
            vendorDTO.PhoneNumber = vendor.PhoneNumber;
            
            return vendorDTO;
        }
        public static IEnumerable<VendorDto> MapToAdminDTOList(IEnumerable<Vendor> vendors)
        {
            return vendors.Select(v => new VendorDto
            {

                Name = v.Name,
                PhoneNumber = v.PhoneNumber,
                 

            }).ToList();


        }
    }
}
