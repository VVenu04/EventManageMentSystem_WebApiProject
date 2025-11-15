using Application.DTOs.Package;
using Application.DTOs.Service;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PackageService:IPackageService
    {
        private readonly IPackageRepository _packageRepo;
        private readonly IServiceRepository _serviceRepo;

        public PackageService(IPackageRepository packageRepo, IServiceRepository serviceRepo)
        {
            _packageRepo = packageRepo;
            _serviceRepo = serviceRepo;
        }

        public async Task<PackageDto> CreatePackageAsync(CreatePackageDto dto, Guid vendorId)
        {
            // 1. Business Logic: Package-இல் services உள்ளனவா?
            if (dto.ServiceIDs == null || !dto.ServiceIDs.Any())
            {
                throw new Exception("A package must contain at least one service.");
            }

            var packageItems = new List<PackageItem>();
            decimal originalServicesPrice = 0;

            // 2. Business Logic: Vendor-க்குச் சொந்தமான services-தானா?
            foreach (var serviceId in dto.ServiceIDs)
            {
                var service = await _serviceRepo.GetByIdAsync(serviceId);
                if (service == null)
                {
                    throw new Exception($"Service with ID {serviceId} not found.");
                }

                // *** மிக முக்கியமான பாதுகாப்புச் சோதனை ***
                if (service.VendorID != vendorId)
                {
                    throw new Exception($"You can only add your own services to a package. Service '{service.Name}' does not belong to you.");
                }

                packageItems.Add(new PackageItem
                {
                    PackageItemID = Guid.NewGuid(),
                    ServiceID = serviceId
                });

                originalServicesPrice += service.Price;
            }

            // 3. Business Logic: Package விலை லாபகரமானதா?
            if (dto.TotalPrice >= originalServicesPrice)
            {
                throw new Exception($"Package price (LKR {dto.TotalPrice}) must be less than the combined original price of services (LKR {originalServicesPrice}).");
            }

            // 4. Package-ஐ உருவாக்கு
            var package = new Package
            {
                PackageID = Guid.NewGuid(),
                Name = dto.Name,
                TotalPrice = dto.TotalPrice,
                VendorID = vendorId,
                Active = true, // Default-ஆக Active
                PackageItems = packageItems // Services-ஐ இணை
            };

            // 5. Database-இல் சேமி
            await _packageRepo.AddAsync(package);

            // 6. DTO-ஆக மாற்றி அனுப்பு
            // (GetByIdAsync-ஐ call செய்வது, Vendor.Name போன்றவற்றைச் சரியாக load செய்யும்)
            var newPackageData = await _packageRepo.GetPackageWithServicesAsync(package.PackageID);
            return MapToPackageDto(newPackageData);
        }

        public async Task<PackageDto> GetPackageByIdAsync(Guid packageId)
        {
            var package = await _packageRepo.GetPackageWithServicesAsync(packageId);
            return MapToPackageDto(package);
        }

        public async Task<IEnumerable<PackageDto>> GetPackagesByVendorIdAsync(Guid vendorId)
        {
            var packages = await _packageRepo.GetPackagesByVendorAsync(vendorId);
            // பல packages-ஐ DTO-ஆக மாற்று
            return packages.Select(MapToPackageDto);
        }


        // --- Helper Method: Manual Mapping ---
        private PackageDto MapToPackageDto(Package package)
        {
            if (package == null) return null;

            return new PackageDto
            {
                PackageID = package.PackageID,
                Name = package.Name,
                TotalPrice = package.TotalPrice,
                Active = package.Active,
                VendorID = package.VendorID,
                VendorName = package.Vendor?.Name, // Vendor-ஐ Include செய்ததால் இது வேலை செய்யும்
                ServicesInPackage = package.PackageItems.Select(item => new SimpleServiceDto
                {
                    ServiceID = item.ServiceID,
                    Name = item.Service?.Name, // Service-ஐ Include செய்ததால் இது வேலை செய்யும்
                    OriginalPrice = item.Service?.Price ?? 0
                }).ToList()
            };
        }
    }
}
