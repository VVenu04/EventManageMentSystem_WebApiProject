using Application.DTOs;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Mapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class VendorService : IVendorService
    {
        private readonly IVendorRepo _vendorRepo;
        public VendorService(IVendorRepo vendorRepo)
        {
            _vendorRepo = vendorRepo;
        }
        public async Task<VendorDto> AddVendorAsync(VendorDto vendorDTO)
        {
            var vendor = VendorMapper.MapToVendor(vendorDTO);
            if (!string.IsNullOrEmpty(vendorDTO.PasswordHash))
            {
                vendor.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vendorDTO.PasswordHash);
            }

            var addedVendor = await _vendorRepo.AddAsync(vendor);
            return VendorMapper.MapToVendorDTO(addedVendor);
        }

        public async  Task DeleteVendorAsync(Guid Id)
        {
            var vendor = await _vendorRepo.GetByIdAsync(c => c.VendorID == Id);
            if (vendor != null)
            {
                await _vendorRepo.DeleteAsync(vendor);
            }

        }

        public async Task<IEnumerable<VendorDto>> GetAllAsync()
        {
            var vendors = await _vendorRepo.GetAllAsync();
            return VendorMapper.MapToVendorDTOList(vendors);
        }

        public async Task<VendorDto> GetVendorAsync(Guid vendorId)
        {
            if (vendorId == Guid.Empty)
            {
                throw new ArgumentException("Invalid vendor ID");
            }
            var vendor = await _vendorRepo.GetByIdAsync(c => c.VendorID == vendorId);
            return VendorMapper.MapToVendorDTO(vendor);
        }
    }
}
