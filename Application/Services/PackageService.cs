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
        private readonly IBookingRepository _bookingRepo;
        private readonly IVendorRepo _vendorRepo; // ✅ Added Vendor Repository

        public PackageService(IPackageRepository packageRepo,
                              IServiceItemRepository serviceRepo,
                              IPackageRequestRepository requestRepo,
                              INotificationService notificationService,
                              IBookingRepository bookingRepo,
                              IVendorRepo vendorRepo) // ✅ Injected here
        {
            _packageRepo = packageRepo;
            _serviceRepo = serviceRepo;
            _requestRepo = requestRepo;
            _notificationService = notificationService;
            _bookingRepo = bookingRepo;
            _vendorRepo = vendorRepo;
        }

        // =========================================================
        // 1. CREATE PACKAGE
        // =========================================================
        public async Task<PackageDto> CreatePackageAsync(CreatePackageDto dto, Guid ownerId)
        {
            var package = new Package
            {
                PackageID = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                VendorID = ownerId,
                Status = "Draft",
                IsActive = false,
                TotalPrice = 0,
                PackageItems = new List<PackageItem>(),
                PackageRequests = new List<PackageRequest>()
            };

            var vendorsToNotify = new List<Guid>();

            if (dto.ServiceItemIDs != null && dto.ServiceItemIDs.Any())
            {
                foreach (var serviceId in dto.ServiceItemIDs)
                {
                    var service = await _serviceRepo.GetByIdAsync(serviceId);
                    if (service == null) continue;

                    package.PackageItems.Add(new PackageItem
                    {
                        PackageItemID = Guid.NewGuid(),
                        PackageID = package.PackageID,
                        ServiceItemID = serviceId,
                        ItemPrice = service.Price,
                        VendorID = service.VendorID
                    });

                    package.TotalPrice += service.Price;

                    if (service.VendorID != ownerId)
                    {
                        package.PackageRequests.Add(new PackageRequest
                        {
                            RequestID = Guid.NewGuid(),
                            PackageID = package.PackageID,
                            SenderVendorID = ownerId,
                            ReceiverVendorID = service.VendorID,
                            Status = "Pending",
                            CreatedAt = DateTime.UtcNow
                        });

                        if (!vendorsToNotify.Contains(service.VendorID))
                        {
                            vendorsToNotify.Add(service.VendorID);
                        }
                    }
                }
            }

            await _packageRepo.AddAsync(package);

            // Fetch Owner Name
            var sender = await _vendorRepo.GetByIdAsync(v => v.VendorID == ownerId);
            string senderName = sender?.Name ?? "A Vendor";

            foreach (var vendorId in vendorsToNotify)
            {
                await _notificationService.SendNotificationAsync(
                    vendorId,
                    $"Collaboration Invite: {senderName} has invited you to join package '{package.Name}'.",
                    "PackageInvite",
                    package.PackageID
                );
            }

            return await GetPackageByIdAsync(package.PackageID);
        }

        // =========================================================
        // 2. ADD SERVICES TO PACKAGE
        // =========================================================
        public async Task AddServicesToPackageAsync(AddServicesToPackageDto dto, Guid ownerId)
        {
            var package = await _packageRepo.GetPackageWithServicesAsync(dto.PackageID);
            if (package == null) throw new Exception("Package not found.");
            if (package.VendorID != ownerId) throw new Exception("Only the package owner can add services.");

            bool dataChanged = false;
            var vendorsToNotify = new List<Guid>();

            foreach (var serviceId in dto.ServiceItemIDs)
            {
                if (package.PackageItems.Any(pi => pi.ServiceItemID == serviceId)) continue;

                var service = await _serviceRepo.GetByIdAsync(serviceId);
                if (service == null) throw new Exception($"Service {serviceId} not found.");

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

                        await _requestRepo.AddAsync(request);

                        if (!vendorsToNotify.Contains(service.VendorID))
                        {
                            vendorsToNotify.Add(service.VendorID);
                        }
                    }
                }
            }

            if (dataChanged)
            {
                await _packageRepo.UpdateAsync(package);
            }

            var sender = await _vendorRepo.GetByIdAsync(v => v.VendorID == ownerId);
            string senderName = sender?.Name ?? "A Vendor";

            foreach (var vendorId in vendorsToNotify)
            {
                await _notificationService.SendNotificationAsync(
                    vendorId,
                    $"Collaboration Invite: {senderName} has added you to package '{package.Name}'.",
                    "PackageInvite",
                    package.PackageID
                );
            }
        }

        // =========================================================
        // 3. PUBLISH PACKAGE
        // =========================================================
        public async Task PublishPackageAsync(Guid packageId, Guid vendorId)
        {
            var package = await _packageRepo.GetPackageWithServicesAsync(packageId);
            if (package == null) throw new Exception("Package not found.");
            if (package.VendorID != vendorId) throw new Exception("Unauthorized.");

            var allRequests = await _requestRepo.GetAllRequestsByPackageAsync(packageId);

            if (allRequests.Any(r => r.Status == "Pending"))
                throw new Exception("Cannot publish: Waiting for partner vendors to respond.");

            if (allRequests.Any(r => r.Status == "Rejected"))
                throw new Exception("Cannot publish: One or more partners have REJECTED your collaboration request.");

            package.Status = "Published";
            package.IsActive = true;
            await _packageRepo.UpdateAsync(package);

            if (package.PackageItems != null && package.PackageItems.Any())
            {
                var collaborators = package.PackageItems
                    .Select(item => item.VendorID)
                    .Distinct()
                    .Where(vid => vid != vendorId)
                    .ToList();

                var publisher = await _vendorRepo.GetByIdAsync(v => v.VendorID == vendorId);
                string publisherName = publisher?.Name ?? "The owner";

                foreach (var collaboratorId in collaborators)
                {
                    // Only notify Accepted partners
                    var request = allRequests.FirstOrDefault(r => r.ReceiverVendorID == collaboratorId);
                    if (request != null && request.Status == "Accepted")
                    {
                        await _notificationService.SendNotificationAsync(
                            collaboratorId,
                            $"Great news! {publisherName} has PUBLISHED the package '{package.Name}'.",
                            "PackagePublished",
                            packageId
                        );
                    }
                }
            }
        }

        // =========================================================
        // 4. INVITE VENDOR
        // =========================================================
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

            var sender = await _vendorRepo.GetByIdAsync(v => v.VendorID == senderId);
            string senderName = sender?.Name ?? "A Vendor";

            await _notificationService.SendNotificationAsync(
                dto.VendorIDToInvite,
                $"Collaboration Invite: {senderName} invited you to join '{package.Name}'!",
                "PackageInvite",
                dto.PackageID
            );
        }

        // =========================================================
        // 5. RESPOND TO INVITE
        // =========================================================
        public async Task RespondToInvitationAsync(Guid requestId, Guid vendorId, bool isAccepted)
        {
            var request = await _requestRepo.GetByIdAsync(requestId);
            if (request == null) throw new Exception("Request not found.");
            if (request.ReceiverVendorID != vendorId) throw new Exception("Unauthorized.");

            var responder = await _vendorRepo.GetByIdAsync(v => v.VendorID == vendorId);
            string responderName = responder?.Name ?? "A partner";

            request.Status = isAccepted ? "Accepted" : "Rejected";
            await _requestRepo.UpdateAsync(request);

            string statusMsg = isAccepted ? "accepted" : "rejected";
            var pkgName = request.Package?.Name ?? (await _packageRepo.GetPackageWithServicesAsync(request.PackageID))?.Name ?? "Unknown Package";

            await _notificationService.SendNotificationAsync(
                request.SenderVendorID,
                $"{responderName} has {statusMsg} your request for package '{pkgName}'.",
                "CollaborationUpdate",
                request.PackageID
            );
        }

        // =========================================================
        // 6. GETTERS
        // =========================================================
        public async Task<IEnumerable<PackageDto>> GetPackagesByVendorIdAsync(Guid vendorId)
        {
            var packages = await _packageRepo.GetPackagesByVendorAsync(vendorId);
            return packages.Select(MapToPackageDto);
        }

        public async Task<IEnumerable<PackageDto>> GetAllPackagesAsync()
        {
            var packages = await _packageRepo.GetAllAsync();
            return packages.Select(MapToPackageDto);
        }

        public async Task<PackageDto> GetPackageByIdAsync(Guid packageId)
        {
            var package = await _packageRepo.GetPackageWithServicesAsync(packageId);
            return MapToPackageDto(package);
        }

        public async Task<IEnumerable<PackageRequestDto>> GetPendingRequestsAsync(Guid vendorId)
        {
            var requests = await _requestRepo.GetPendingRequestsByVendorAsync(vendorId);
            var requestDtos = new List<PackageRequestDto>();

            foreach (var req in requests)
            {
                var sender = req.SenderVendor;
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
            var request = await _requestRepo.GetRequestAsync(packageId, requestingVendorId);
            if (request == null) throw new Exception("Access Denied.");

            var package = await _packageRepo.GetPackageWithServicesAsync(packageId);
            return MapToPackageDto(package);
        }

        // =========================================================
        // 7. DELETE PACKAGE
        // =========================================================
        public async Task DeletePackageAsync(Guid packageId, Guid vendorId)
        {
            var package = await _packageRepo.GetPackageWithServicesAsync(packageId);
            if (package == null) throw new Exception("Package not found.");
            if (package.VendorID != vendorId) throw new Exception("Unauthorized.");

            bool hasBookings = await _bookingRepo.IsPackageBookedAsync(packageId);
            if (hasBookings) throw new Exception("Cannot delete: This package has existing bookings.");

            var vendorsToNotify = new List<Guid>();
            if (package.PackageItems != null)
            {
                vendorsToNotify = package.PackageItems
                    .Select(pi => pi.VendorID)
                    .Distinct()
                    .Where(id => id != vendorId)
                    .ToList();
            }

            await _packageRepo.DeleteAsync(packageId);

            var deleter = await _vendorRepo.GetByIdAsync(v => v.VendorID == vendorId);
            string deleterName = deleter?.Name ?? "The owner";

            foreach (var collabId in vendorsToNotify)
            {
                await _notificationService.SendNotificationAsync(
                    collabId,
                    $"The package '{package.Name}' has been deleted by {deleterName}.",
                    "PackageDeleted",
                    Guid.Empty
                );
            }
        }

        // --- Helper ---
        private PackageDto MapToPackageDto(Package package)
        {
            if (package == null) return null;

            var collectedImages = new List<string>();
            if (package.PackageItems != null)
            {
                foreach (var item in package.PackageItems)
                {
                    if (item.Service != null && item.Service.ServiceImages != null && item.Service.ServiceImages.Any())
                    {
                        collectedImages.Add(item.Service.ServiceImages.First().ImageUrl);
                    }
                }
            }

            return new PackageDto
            {
                PackageID = package.PackageID,
                Name = package.Name,
                Description = package.Description,
                TotalPrice = package.TotalPrice,
                Active = package.IsActive,
                VendorID = package.VendorID,
                VendorName = package.Vendor?.Name ?? "Unknown Vendor",
                PackageImages = collectedImages,
                ServicesInPackage = package.PackageItems.Select(item => new SimpleServiceDto
                {
                    ServiceItemID = item.ServiceItemID ?? Guid.Empty,
                    Name = item.Service?.Name ?? "Unknown Service",
                    OriginalPrice = item.ItemPrice,
                    ImageUrl = item.Service?.ServiceImages?.FirstOrDefault()?.ImageUrl
                }).ToList()
            };
        }
    }
}