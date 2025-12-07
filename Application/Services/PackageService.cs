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

        // 1. Create Package (Optimized: One-Shot Save)
        public async Task<PackageDto> CreatePackageAsync(CreatePackageDto dto, Guid ownerId)
        {
            // A. Initialize the Package
            var package = new Package
            {
                PackageID = Guid.NewGuid(),
                Name = dto.Name,
                VendorID = ownerId,
                Status = "Draft",
                IsActive = false,
                TotalPrice = 0,
                PackageItems = new List<PackageItem>(), // Initialize list
                PackageRequests = new List<PackageRequest>() // Initialize list
            };

            // B. Add Services to the Object (IN MEMORY - NO DB CALLS YET)
            if (dto.ServiceItemIDs != null && dto.ServiceItemIDs.Any())
            {
                foreach (var serviceId in dto.ServiceItemIDs)
                {
                    var service = await _serviceRepo.GetByIdAsync(serviceId);
                    if (service == null) continue; // Skip invalid services

                    // 1. Add Item
                    package.PackageItems.Add(new PackageItem
                    {
                        PackageItemID = Guid.NewGuid(),
                        PackageID = package.PackageID,
                        ServiceItemID = serviceId,
                        ItemPrice = service.Price,
                        VendorID = service.VendorID
                    });

                    // 2. Update Total Price
                    package.TotalPrice += service.Price;

                    // 3. Handle Collaboration Request (If needed)
                    if (service.VendorID != ownerId)
                    {
                        // Add to the Package's collection directly
                        package.PackageRequests.Add(new PackageRequest
                        {
                            RequestID = Guid.NewGuid(),
                            PackageID = package.PackageID,
                            SenderVendorID = ownerId,
                            ReceiverVendorID = service.VendorID,
                            Status = "Pending",
                            CreatedAt = DateTime.UtcNow
                        });

                        // Notify (Fire and Forget)
                        _ = _notificationService.SendNotificationAsync(
                            service.VendorID,
                            $"Collaboration Invite: You have a new request for package '{package.Name}'.",
                            "PackageInvite",
                            package.PackageID
                        );
                    }
                }
            }

            // C. SAVE EVERYTHING ONCE
            // This will save the Package, The Items, AND The Requests in one transaction.
            await _packageRepo.AddAsync(package);

            return await GetPackageByIdAsync(package.PackageID);
        }

        // 2. Add Services (With Collaboration Logic)
        public async Task AddServicesToPackageAsync(AddServicesToPackageDto dto, Guid ownerId)
        {
            // 1. Get Package (Tracking is ON by default)
            var package = await _packageRepo.GetPackageWithServicesAsync(dto.PackageID);

            if (package == null) throw new Exception("Package not found.");
            if (package.VendorID != ownerId) throw new Exception("Only the package owner can add services.");

            bool dataChanged = false;

            foreach (var serviceId in dto.ServiceItemIDs)
            {
                // Check for duplicates
                if (package.PackageItems.Any(pi => pi.ServiceItemID == serviceId)) continue;

                var service = await _serviceRepo.GetByIdAsync(serviceId);
                if (service == null) throw new Exception($"Service {serviceId} not found.");

                // 2. Add Item to Memory
                package.PackageItems.Add(new PackageItem
                {
                    PackageItemID = Guid.NewGuid(),
                    PackageID = package.PackageID,
                    ServiceItemID = serviceId,
                    ItemPrice = service.Price,
                    VendorID = service.VendorID
                });

                package.TotalPrice += service.Price;
                dataChanged = true;

                // 3. Handle External Vendor Request
                if (service.VendorID != ownerId)
                {
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

                        // Add Request to Context directly. 
                        // Do NOT use _requestRepo.AddAsync here if it calls SaveChanges immediately.
                        // It is safer to rely on the final package update to save everything.
                        await _requestRepo.AddAsync(request);
                    }
                }
            }

            // 4. Final Save
            if (dataChanged)
            {
                // Because 'package' is tracked, this updates Package, Items, and Requests
                await _packageRepo.UpdateAsync(package);
            }
        }

        // 3. Publish Package (Check Requests)
        public async Task PublishPackageAsync(Guid packageId, Guid vendorId)
        {
            var package = await _packageRepo.GetPackageWithServicesAsync(packageId);
            if (package == null) throw new Exception("Package not found.");
            if (package.VendorID != vendorId) throw new Exception("Unauthorized.");

            // --- FIX: Use the new method that checks by PackageID ---
            var pendingRequests = await _requestRepo.GetPendingRequestsByPackageAsync(packageId);

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