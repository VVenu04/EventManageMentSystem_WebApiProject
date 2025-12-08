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
    public class VendorService : IVendorService
    {
        private readonly IVendorRepo _vendorRepo;
        public VendorService(IVendorRepo vendorRepo)
        {
            _vendorRepo = vendorRepo;
        }

        public async Task<VendorDto> AddVendorAsync(VendorDto vendorDTO)
        {
            if (vendorDTO == null)
            {
                throw new ArgumentNullException(nameof(vendorDTO));
            }

            var vendor = VendorMapper.MapToVendor(vendorDTO);

            //  HASH THE PASSWORD 
            if (!string.IsNullOrEmpty(vendorDTO.Password))
            {
                vendor.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vendorDTO.Password);
            }
          

            var addedVendor = await _vendorRepo.AddAsync(vendor);
            return VendorMapper.MapToVendorDTO(addedVendor);
        }

        //  DELETE VENDOR 
        public async Task DeleteVendorAsync(Guid Id)
        {
            var vendor = await _vendorRepo.GetByIdAsync(v => v.VendorID == Id);
            if (vendor == null)
            {
         
                throw new Exception($"Vendor with ID {Id} not found.");
            }
            await _vendorRepo.DeleteAsync(vendor);
        }

        // GET ALL VENDORS 
        public async Task<IEnumerable<VendorDto>> GetAllAsync()
        {
            var vendors = await _vendorRepo.GetAllAsync();
            return VendorMapper.MapToVendorDTOList(vendors);
        }

        //  GET VENDOR BY ID 
        public async Task<VendorDto> GetVendorAsync(Guid vendorId)
        {
            var vendor = await _vendorRepo.GetByIdAsync(v => v.VendorID == vendorId);
            return VendorMapper.MapToVendorDTO(vendor);
        }

        public async Task<bool> UpdateVendorLogoAsync(Guid vendorId, string logoUrl)
        {
            var vendor = await _vendorRepo.GetByIdAsync(v => v.VendorID == vendorId); // அல்லது _context.Vendors.Find(vendorId)
            if (vendor == null) return false;

            vendor.Logo = logoUrl; // URL-ஐ set பண்றோம்
            vendor.UpdatedAt = DateTime.UtcNow;

            await _vendorRepo.UpdateAsync(vendor); // Save Changes
            return true;
        }

        public async Task<bool> UpdateVendorProfilePhotoAsync(Guid vendorId, string photoUrl)
        {
            var vendor = await _vendorRepo.GetByIdAsync(v => v.VendorID == vendorId);
            if (vendor == null) return false;

            vendor.ProfilePhoto = photoUrl;
            vendor.UpdatedAt = DateTime.UtcNow;

            await _vendorRepo.UpdateAsync(vendor);
            return true;
        }
    }
}