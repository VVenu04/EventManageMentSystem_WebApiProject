using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface IVendorService
    {
        Task<VendorDto> AddVendorAsync(VendorDto vendorDTO);
        Task DeleteVendorAsync(Guid Id);
        Task<VendorDto> GetVendorAsync(Guid vendorId);
        Task<IEnumerable<VendorDto>> GetAllAsync();
        Task<bool> UpdateVendorLogoAsync(Guid vendorId, string logoUrl);
        Task<bool> UpdateVendorProfilePhotoAsync(Guid vendorId, string photoUrl);
    }
}
