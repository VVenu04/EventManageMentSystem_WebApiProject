using Application.DTOs.Package;
using Application.DTOs.Service;
using Application.DTOs.ServiceItem;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepo;
        private readonly IServiceItemRepository _serviceRepo;
        private readonly IPackageRequestRepository _requestRepo;
        private readonly INotificationService _notificationService;

        public PackageService(IPackageRepository packageRepo,
                              IServiceItemRepository serviceRepo,
                              IPackageRequestRepository requestRepo,
                              INotificationService notificationService)
        {
            _packageRepo = packageRepo;
            _serviceRepo = serviceRepo;
            _requestRepo = requestRepo;
            _notificationService = notificationService;
        }

        // 1. Create Package (Draft)
        public async Task<PackageDto> CreatePackageAsync(CreatePackageDto dto, Guid ownerId)
        {
            var package = new Package
            {
                PackageID = Guid.NewGuid(),
                Name = dto.Name,
                VendorID = ownerId,
                Status = "Draft",
                IsActive = false,
                TotalPrice = 0
            };

            await _packageRepo.AddAsync(package);

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

        // 2. Add Services (With Collaboration Logic)
        public async Task AddServicesToPackageAsync(AddServicesToPackageDto dto, Guid ownerId)
        {
            var package = await _packageRepo.GetPackageWithServicesAsync(dto.PackageID);
            if (package == null) throw new Exception("Package not found.");

            if (package.VendorID != ownerId) throw new Exception("Only the package owner can add services.");

            foreach (var serviceId in dto.ServiceItemIDs)
            {
                // Check duplicate service
                if (package.PackageItems.Any(pi => pi.ServiceItemID == serviceId)) continue;

                var service = await _serviceRepo.GetByIdAsync(serviceId);
                if (service == null) throw new Exception("Service not found.");

                // A. Own Service -> Add Directly
                if (service.VendorID == ownerId)
                {
                    package.PackageItems.Add(new PackageItem
                    {
                        PackageItemID = Guid.NewGuid(),
                        PackageID = package.PackageID,
                        ServiceItemID = serviceId,
                        ItemPrice = service.Price,
                        VendorID = service.VendorID // Store Vendor ID
                    });
                    package.TotalPrice += service.Price;
                }
                // B. Other Vendor Service -> Create Request & Add (Pending)
                else
                {
                    package.PackageItems.Add(new PackageItem
                    {
                        PackageItemID = Guid.NewGuid(),
                        PackageID = package.PackageID,
                        ServiceItemID = serviceId,
                        ItemPrice = service.Price,
                        VendorID = service.VendorID // Store Vendor ID
                    });
                    package.TotalPrice += service.Price;

                    // Collaboration Request Check
                    var existingRequest = await _requestRepo.GetRequestAsync(package.PackageID, service.VendorID);

                    if (existingRequest == null)
                    {
                        var request = new PackageRequest
                        {
                            RequestID = Guid.NewGuid(),
                            PackageID = package.PackageID,
                            SenderVendorID = ownerId,
                            ReceiverVendorID = service.VendorID,
                            Status = "Pending",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _requestRepo.AddAsync(request);

                        await _notificationService.SendNotificationAsync(
                            service.VendorID,
                            $"Collaboration Invite: '{package.Vendor?.Name}' wants to add your service '{service.Name}' to package '{package.Name}'.",
                            "PackageInvite",
                            package.PackageID
                        );
                    }
                }
            }
            await _packageRepo.UpdateAsync(package);
        }

        // 3. Publish Package (Check Requests)
        public async Task PublishPackageAsync(Guid packageId, Guid vendorId)
        {
            var package = await _packageRepo.GetPackageWithServicesAsync(packageId);
            if (package == null) throw new Exception("Package not found.");
            if (package.VendorID != vendorId) throw new Exception("Unauthorized.");

            // Validate pending collaborations
            var pendingRequests = await _requestRepo.GetPendingRequestsForVendorAsync(packageId); // Need this method in Repo
            if (pendingRequests.Any())
            {
                throw new Exception("Cannot publish: Waiting for partner vendors to accept collaboration.");
            }

            package.Status = "Published";
            package.IsActive = true;
            await _packageRepo.UpdateAsync(package);
        }

        // 4. Invite Vendor (Explicit)
        public async Task InviteVendorAsync(InviteVendorDto dto, Guid senderId)
        {
            var package = await _packageRepo.GetPackageWithServicesAsync(dto.PackageID);
            if (package == null) throw new Exception("Package not found.");
            if (package.VendorID != senderId) throw new Exception("Only owner can invite.");

            var existingRequest = await _requestRepo.GetRequestAsync(dto.PackageID, dto.VendorIDToInvite);
            if (existingRequest != null) throw new Exception("Request already sent.");

            var request = new PackageRequest
            {
                RequestID = Guid.NewGuid(),
                PackageID = dto.PackageID,
                SenderVendorID = senderId,
                ReceiverVendorID = dto.VendorIDToInvite,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _requestRepo.AddAsync(request);

            await _notificationService.SendNotificationAsync(
                dto.VendorIDToInvite,
                "You have been invited to collaborate!",
                "PackageInvite",
                dto.PackageID
            );
        }

        // 5. Respond to Invitation
        public async Task RespondToInvitationAsync(Guid requestId, Guid vendorId, bool isAccepted)
        {
            var request = await _requestRepo.GetByIdAsync(requestId);
            if (request == null) throw new Exception("Request not found.");

            if (request.ReceiverVendorID != vendorId) throw new Exception("Unauthorized.");

            request.Status = isAccepted ? "Accepted" : "Rejected";
            await _requestRepo.UpdateAsync(request);

            // Notify Owner
            string statusMsg = isAccepted ? "accepted" : "rejected";
            await _notificationService.SendNotificationAsync(
                request.SenderVendorID,
                $"Vendor has {statusMsg} your collaboration request.",
                "CollaborationUpdate",
                request.PackageID
            );
        }

        // 6. Get Vendor Packages
        public async Task<IEnumerable<PackageDto>> GetPackagesByVendorIdAsync(Guid vendorId)
        {
            var packages = await _packageRepo.GetPackagesByVendorAsync(vendorId);
            return packages.Select(MapToPackageDto);
        }

        // 7. Get All Packages
        public async Task<IEnumerable<PackageDto>> GetAllPackagesAsync()
        {
            var packages = await _packageRepo.GetAllAsync();
            return packages.Select(MapToPackageDto);
        }

        // 8. Get Package By ID
        public async Task<PackageDto> GetPackageByIdAsync(Guid packageId)
        {
            var package = await _packageRepo.GetPackageWithServicesAsync(packageId);
            return MapToPackageDto(package);
        }

        // --- Helper Method ---
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
                VendorName = package.Vendor?.Name ?? "Unknown Vendor",
                ServicesInPackage = package.PackageItems.Select(item => new SimpleServiceDto
                {
                    ServiceItemID = item.ServiceItemID ?? Guid.Empty,
                    Name = item.Service?.Name ?? "Unknown Service",
                    OriginalPrice = item.ItemPrice
                }).ToList()
            };
        }

        public async Task<IEnumerable<PackageRequestDto>> GetPendingRequestsAsync(Guid vendorId)
        {
            var requests = await _requestRepo.GetPendingRequestsByVendorAsync(vendorId);

            // Map Entity to DTO
            var requestDtos = new List<PackageRequestDto>();

            foreach (var req in requests)
            {
                // Sender Info (Navigation Property இல்லையென்றால் தனியாக எடுக்கவும்)
                var sender = req.SenderVendor;
                // அல்லது: var sender = await _vendorRepo.GetByIdAsync(req.SenderVendorID);

                requestDtos.Add(new PackageRequestDto
                {
                    RequestID = req.RequestID,
                    PackageID = req.PackageID,
                    PackageName = req.Package?.Name ?? "Unknown Package",
                    SenderVendorID = req.SenderVendorID,
                    SenderName = sender?.Name ?? "Unknown Vendor",
                    SenderLogo = sender?.Logo,
                    Status = req.Status,
                    CreatedAt = req.CreatedAt
                });
            }

            return requestDtos;
        }
        public async Task<PackageDto> GetPackagePreviewForCollabAsync(Guid packageId, Guid requestingVendorId)
        {
            // 1. அந்த பேக்கேஜுக்கான Request இந்த வெண்டருக்கு வந்துள்ளதா எனச் சரிபார்
            var request = await _requestRepo.GetRequestAsync(packageId, requestingVendorId);

            // Request இல்லை என்றால் பார்க்க அனுமதி இல்லை
            if (request == null)
            {
                throw new Exception("Access Denied: You do not have an invitation for this package.");
            }

            // 2. பேக்கேஜ் விபரங்களை எடு
            var package = await _packageRepo.GetPackageWithServicesAsync(packageId);

            // 3. DTO-ாக மாற்றி அனுப்பு (ஏற்கனவே உள்ள Helper-ஐப் பயன்படுத்தலாம்)
            return MapToPackageDto(package);
        }
    }
}