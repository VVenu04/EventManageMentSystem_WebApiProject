using Application.DTOs.Package;
using Application.DTOs.Service;
using Application.DTOs.ServiceItem;
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
        private readonly IServiceItemRepository _serviceRepo;
        private readonly IPackageRequestRepository _requestRepo;
        private readonly INotificationService _notificationService; // <-- 1. Inject செய்யவும்

        public PackageService(IPackageRepository packageRepo, IServiceItemRepository serviceRepo, IPackageRequestRepository requestRepo)
        {
            _packageRepo = packageRepo;
            _serviceRepo = serviceRepo;
            _requestRepo = requestRepo;

        }

        public async Task<PackageDto> CreatePackageAsync(CreatePackageDto dto, Guid ownerId)
        {
            // ... (பழைய logic: Package உருவாக்குவது, ஆனால் Status = "Draft") ...
            var package = new Package
            {
                PackageID = Guid.NewGuid(),
                Name = dto.Name,
                VendorID = ownerId,
                Status = "Draft", // முதலில் Draft ஆக இருக்கும்
                IsActive = false,
                TotalPrice = 0 // பிறகு Update செய்யலாம்
            };

            // (Services சேர்க்கும் logic இங்கே வரும்...)
            await _packageRepo.AddAsync(package);
            
            // Serviceகளைச் சேர்க்க தனி method-ஐ call செய்யலாம்
            if (dto.ServiceItemIDs != null && dto.ServiceItemIDs.Any())
            {
                await AddServicesToPackageAsync(new AddServicesToPackageDto
                {
                    PackageID = package.PackageID,
                    ServiceItemIDs = dto.ServiceItemIDs
                }, ownerId);
            }

            return await GetPackageByIdAsync(package.PackageID);
        }

        // 2. Invite Vendor (Vendor A அழைப்பு விடுக்கிறார்)
        public async Task InviteVendorAsync(InviteVendorDto dto, Guid senderId)
        {
            var package = await _packageRepo.GetPackageWithServicesAsync(dto.PackageID);
            if (package == null) throw new Exception("Package not found.");

            // Owner மட்டும்தான் Invite செய்ய முடியும்
            if (package.VendorID != senderId)
                throw new Exception("Only the package owner can invite others.");

            // ஏற்கனவே Invite செய்துள்ளாரா?
            var existingRequest = await _requestRepo.GetRequestAsync(dto.PackageID, dto.VendorIDToInvite);
            if (existingRequest != null)
                throw new Exception("Request already sent to this vendor.");

            var request = new PackageRequest
            {
                RequestID = Guid.NewGuid(),
                PackageID = dto.PackageID,
                SenderVendorID = senderId,
                ReceiverVendorID = dto.VendorIDToInvite,
                Status = "Pending"
            };

            await _requestRepo.AddAsync(request);
            await _notificationService.SendNotificationAsync(
                dto.VendorIDToInvite,
                "You have been invited to collaborate on a new package!",
                "PackageInvite",
                dto.PackageID
               );
        }

        // 3. Accept/Reject Invitation (Vendor B செயல்படுகிறார்)
        public async Task RespondToInvitationAsync(Guid requestId, Guid vendorId, bool isAccepted)
        {
            var request = await _requestRepo.GetByIdAsync(requestId);
            if (request == null) throw new Exception("Request not found.");

            if (request.ReceiverVendorID != vendorId)
                throw new Exception("Unauthorized to respond to this request.");

            request.Status = isAccepted ? "Accepted" : "Rejected";
            await _requestRepo.UpdateAsync(request);
        }

        // 4. Add Services (Vendor A மற்றும் Vendor B இருவரும் செய்யலாம்)
        public async Task AddServicesToPackageAsync(AddServicesToPackageDto dto, Guid vendorId)
        {
            // Permission Check: இவர் Owner-ஆ அல்லது Accepted Collaborator-ஆ?
            bool isAuthorized = await _requestRepo.IsVendorApprovedForPackageAsync(dto.PackageID, vendorId);
            if (!isAuthorized)
            {
                throw new Exception("You are not authorized to add services to this package. Wait for approval.");
            }

            var package = await _packageRepo.GetPackageWithServicesAsync(dto.PackageID);

            if (package == null)
            {
                throw new Exception("Package not found.");
            }
            if (package.PackageItems == null)
            {
                package.PackageItems = new List<PackageItem>();
            }

            foreach (var serviceId in dto.ServiceItemIDs)
            {
                var service = await _serviceRepo.GetByIdAsync(serviceId);
                if (service == null) throw new Exception("Service not found.");

                if (service.VendorID != vendorId)
                {
                    throw new Exception($"You can only add YOUR services. Service '{service.Name}' is not yours.");
                }

                if (!package.PackageItems.Any(pi => pi.ServiceItemID == serviceId))
                {
                    package.PackageItems.Add(new PackageItem
                    {
                        PackageItemID = Guid.NewGuid(),
                        PackageID = package.PackageID,
                        ServiceItemID = serviceId
                    });

                    package.TotalPrice += service.Price;
                }
            }

            // Save changes
            await _packageRepo.UpdateAsync(package); // UpdateAsync-ஐ implement செய்யவும்
        }

        // 5. Publish Package (Vendor A only)
        public async Task PublishPackageAsync(Guid packageId, Guid vendorId)
        {
            var package = await _packageRepo.GetPackageWithServicesAsync(packageId);
            if (package == null)
            {
                throw new Exception("Package not found.");
            }
            if (package.VendorID != vendorId) throw new Exception("Only owner can publish.");

            package.Status = "Published";
            package.IsActive = true;
            await _packageRepo.UpdateAsync(package);
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
                Active = package.IsActive,
                VendorID = package.VendorID,
                VendorName = package.Vendor?.Name, // Vendor-ஐ Include செய்ததால் இது வேலை செய்யும்
                ServicesInPackage = package.PackageItems.Select(item => new SimpleServiceDto
                {
                    ServiceItemID = item.ServiceItemID,
                    Name = item.Service?.Name, // Service-ஐ Include செய்ததால் இது வேலை செய்யும்
                    OriginalPrice = item.Service?.Price ?? 0
                }).ToList()
            };
        }
    }
}
