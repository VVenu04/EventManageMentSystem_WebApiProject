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

        public PackageService(IPackageRepository packageRepo,
                              IServiceItemRepository serviceRepo,
                              IPackageRequestRepository requestRepo,
                              INotificationService notificationService,
                              IBookingRepository bookingRepo)
        {
            _packageRepo = packageRepo;
            _serviceRepo = serviceRepo;
            _requestRepo = requestRepo;
            _notificationService = notificationService;
            _bookingRepo = bookingRepo;
        }

        // 1. Create Package (FIXED: Threading Issue Resolved)
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
                PackageItems = new List<PackageItem>(),
                PackageRequests = new List<PackageRequest>()
            };

            // 
            // <--- CHANGE 1: Create a list to hold vendors we need to notify later
            var vendorsToNotify = new List<Guid>();

            // B. Add Services to the Object
            if (dto.ServiceItemIDs != null && dto.ServiceItemIDs.Any())
            {
                foreach (var serviceId in dto.ServiceItemIDs)
                {
                    var service = await _serviceRepo.GetByIdAsync(serviceId);
                    if (service == null) continue;

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

                    // 3. Handle Collaboration Request
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

                        // <--- CHANGE 2: Do NOT send notification here. Just add to the list.
                        // This prevents the "Second operation started on this context" error.
                        if (!vendorsToNotify.Contains(service.VendorID))
                        {
                            vendorsToNotify.Add(service.VendorID);
                        }
                    }
                }
            }

            // C. SAVE EVERYTHING ONCE (Synchronously await this)
            await _packageRepo.AddAsync(package);

            // <--- CHANGE 3: Send Notifications NOW (After DB is free)
            foreach (var vendorId in vendorsToNotify)
            {
                // We await this to ensure it runs safely
                await _notificationService.SendNotificationAsync(
                    vendorId,
                    $"Collaboration Invite: You have a new request for package '{package.Name}'.",
                    "PackageInvite",
                    package.PackageID
                );
            }

            return await GetPackageByIdAsync(package.PackageID);
        }
        public async Task LeaveFromPackage(LeavePackageDto dto)
        {
            var package = await GetPackageByIdAsync(dto.PackageID);
            if (package == null) throw new Exception("Package not found.");
            var leaver = await _packageRepo.GetPackagesByVendorAsync(package.VendorID);
             foreach(var item in leaver)
            {
               if( dto.VendorIDToLeave == item.VendorID)
                {
                    // oru sikal package table la eruth vendor ah remove pana function seija(repo la seijonum antha function) many to many issue eruku 
                    await _packageRepo.DeletevendorAsync(dto.PackageID, dto.VendorIDToLeave);
                }  
                
            }

        }
        // 2. Add Services (With Collaboration Logic)
        public async Task AddServicesToPackageAsync(AddServicesToPackageDto dto, Guid ownerId)
        {
            // 1. Get Package
            var package = await _packageRepo.GetPackageWithServicesAsync(dto.PackageID);

            if (package == null) throw new Exception("Package not found.");
            if (package.VendorID != ownerId) throw new Exception("Only the package owner can add services.");

            bool dataChanged = false;

            // <--- CHANGE 4: Apply same logic here (List instead of immediate send)
            var vendorsToNotify = new List<Guid>();

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

                        await _requestRepo.AddAsync(request);

                        // Add to list to notify later
                        if (!vendorsToNotify.Contains(service.VendorID))
                        {
                            vendorsToNotify.Add(service.VendorID);
                        }
                    }
                }
            }

            // 4. Final Save
            if (dataChanged)
            {
                await _packageRepo.UpdateAsync(package);
            }

            // <--- CHANGE 5: Send Notifications after update is complete
            foreach (var vendorId in vendorsToNotify)
            {
                await _notificationService.SendNotificationAsync(
                    vendorId,
                    $"Collaboration Invite: You have a new request for package '{package.Name}'.",
                    "PackageInvite",
                    package.PackageID
                );
            }
        }

        // 3. Publish Package (Updated with Notification Logic)
        public async Task PublishPackageAsync(Guid packageId, Guid vendorId)
        {
            // 1. Get Package with Items
            var package = await _packageRepo.GetPackageWithServicesAsync(packageId);
            if (package == null) throw new Exception("Package not found.");
            if (package.VendorID != vendorId) throw new Exception("Unauthorized.");

            // 2. Ensure all requests are accepted
            var pendingRequests = await _requestRepo.GetPendingRequestsByPackageAsync(packageId);

            if (pendingRequests.Any())
            {
                throw new Exception("Cannot publish: Waiting for partner vendors to accept collaboration.");
            }

            // 3. Update Status
            package.Status = "Published";
            package.IsActive = true;
            await _packageRepo.UpdateAsync(package);

            if (package.PackageItems != null && package.PackageItems.Any())
            {
                // Find all unique vendors involved in this package, excluding the owner 
                var collaborators = package.PackageItems
                    .Select(item => item.VendorID) // Ensure PackageItem has VendorID populated
                    .Distinct()
                    .Where(vid => vid != vendorId) // Don't notify the person publishing it
                    .ToList();

                foreach (var collaboratorId in collaborators)
                {
                    string message = $"Great news! The package '{package.Name}' (which includes your service) has been PUBLISHED by {package.Vendor?.Name ?? "the owner"}.";

                    await _notificationService.SendNotificationAsync(
                        collaboratorId,       // Receiver 
                        message,              // Message
                        "PackagePublished",   // Notification Type
                        packageId             // Related Entity ID
                    );
                }
            }
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

            if (request == null)
            {
                throw new Exception("Access Denied: You do not have an invitation for this package.");
            }

            var package = await _packageRepo.GetPackageWithServicesAsync(packageId);
            return MapToPackageDto(package);
        }


        // NEW METHOD: Delete Package
        // ==============================================
        public async Task DeletePackageAsync(Guid packageId, Guid vendorId)
        {
            // 1. Get Package details (needed to find collaborators)
            var package = await _packageRepo.GetPackageWithServicesAsync(packageId);

            if (package == null) throw new Exception("Package not found.");

            // 2. Verify Ownership
            if (package.VendorID != vendorId)
                throw new Exception("Unauthorized: You can only delete your own packages.");

            // 3. CHECK BOOKINGS (Crucial Step)
            bool hasBookings = await _bookingRepo.IsPackageBookedAsync(packageId);
            if (hasBookings)
            {
                throw new Exception("Cannot delete: This package has existing bookings. You can only deactivate it.");
            }

            // 4. Collect Collaborators to Notify (Before deleting)
            var vendorsToNotify = new List<Guid>();
            if (package.PackageItems != null)
            {
                vendorsToNotify = package.PackageItems
                    .Select(pi => pi.VendorID)
                    .Distinct()
                    .Where(id => id != vendorId) // Exclude owner
                    .ToList();
            }

            // 5. Delete from DB
            await _packageRepo.DeleteAsync(packageId);

            // 6. Notify Collaborators
            foreach (var collabId in vendorsToNotify)
            {
                await _notificationService.SendNotificationAsync(
                    collabId,
                    $"The package '{package.Name}' has been deleted by the owner.",
                    "PackageDeleted",
                    Guid.Empty // No related ID because it's deleted
                );
            }
        }
    
}
}