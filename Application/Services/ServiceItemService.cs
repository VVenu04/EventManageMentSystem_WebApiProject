using Application.DTOs.Service;
using Application.DTOs.ServiceItem;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Mapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ServiceItemService : IServiceItemService
    {
        private readonly IServiceItemRepository _serviceRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IEventRepo _eventRepo;
        private readonly IPhotoService _photoService; 
        private readonly IAuthRepository _authRepo;

        public ServiceItemService(
            IServiceItemRepository serviceRepo, ICategoryRepository categoryRepo, IEventRepo eventRepo, IPhotoService photoService,IAuthRepository authRepository)
        {
            _serviceRepo = serviceRepo;
            _categoryRepo = categoryRepo;
            _eventRepo = eventRepo;
            _photoService = photoService;
            _authRepo = authRepository;
        }


        public async Task<ServiceItemDto> CreateServiceAsync(CreateServiceDto dto, List<IFormFile> images, Guid vendorId)
        {
            var vendor = await _authRepo.GetVendorByIdAsync(vendorId);
            var verify = vendor.IsVerified;
            if (verify == false)
            {
                throw new Exception("Your account is not verified. You cannot create a service.");
            }
            // 1. Image Validation (File Count Check)
            if (images == null || !images.Any())
                throw new Exception("You must upload at least one photo for the service.");

            if (images.Count > 5)
                throw new Exception("You cannot add more than 5 photos per service.");

            // 2. Category & Event Validation (பழைய Code அப்படியே...)
            var category = await _categoryRepo.GetByIdAsync(dto.CategoryID);
            if (category == null) throw new Exception($"Category not found.");

            var selectedEvents = new List<Event>();
            if (dto.EventIDs != null)
            {
                foreach (var evtId in dto.EventIDs)
                {
                    var evt = await _eventRepo.GetByIdAsync(evtId);
                    if (evt != null) selectedEvents.Add(evt);
                }
            }

            // --- 3. CLOUDINARY UPLOAD LOGIC (இதுதான் முக்கியம்!) ---
            var serviceImages = new List<ServiceImage>();

            // போட்டோ ஒவ்வொன்றாக Cloudinary-ல் ஏற்றுகிறோம்
            for (int i = 0; i < images.Count; i++)
            {
                var file = images[i];

                // IPhotoService-ஐ வைத்து Upload செய்கிறோம் (இதை Constructor-ல் Inject செய்ய வேண்டும்)
                var uploadResult = await _photoService.AddPhotoAsync(file);

                if (uploadResult.Error != null)
                    throw new Exception($"Image upload failed: {uploadResult.Error.Message}");

                // Cloudinary தந்த URL-ஐ List-ல் சேர்க்கிறோம்
                serviceImages.Add(new ServiceImage
                {
                    ServiceImageID = Guid.NewGuid(),
                    ImageUrl = uploadResult.SecureUrl.AbsoluteUri, // Cloudinary URL
                    IsCover = (i == 0) // முதல் போட்டோ Cover Photo
                });
            }

            // 4. Create Service Entity
            var service = new ServiceItem
            {
                ServiceItemID = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Location = dto.Location,
                CategoryID = dto.CategoryID,
                Events = selectedEvents,
                EventPerDayLimit = dto.EventPerDayLimit,
                TimeLimit = dto.TimeLimit,
                VendorID = vendorId,
                Active = true,
                ServiceImages = serviceImages // Cloudinary URLs உள்ள List
            };

            // 5. Save to Database
            await _serviceRepo.AddAsync(service);

            // 6. Return DTO
            var fullServiceDetails = await _serviceRepo.GetByIdAsync(service.ServiceItemID);
            return ServiceMapper.MapToServiceDto(fullServiceDetails);
        }

        public async Task DeleteServiceAsync(Guid serviceId, Guid vendorId)
        {
            // 1. Get Service with Details (Images, Events)
            var service = await _serviceRepo.GetByIdAsync(serviceId);

            if (service == null) throw new Exception("Service not found");

            // 2. Verify Ownership
            if (service.VendorID != vendorId) throw new Exception("Unauthorized to delete this service");

            // 3. Check Dependency (Package)
            if (await _serviceRepo.IsServiceInAnyPackageAsync(serviceId))
                throw new Exception($"Cannot delete '{service.Name}' because it is part of one or more Packages.");

            // 4. Delete
            await _serviceRepo.DeleteAsync(service);
        }

        public async Task UpdateServiceAsync(Guid serviceId, UpdateServiceDto dto, List<IFormFile> images, Guid vendorId)
        {
            var service = await _serviceRepo.GetByIdWithDetailsAsync(serviceId);
            if (service == null) throw new Exception("Service not found");
            if (service.VendorID != vendorId) throw new Exception("Unauthorized");

            // 1. Update Properties
            service.Name = dto.Name;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.Location = dto.Location;
            service.CategoryID = dto.CategoryID;
            service.EventPerDayLimit = dto.EventPerDayLimit;
            service.TimeLimit = dto.TimeLimit;

            // 2. Update Events
            service.Events.Clear();
            if (dto.EventIDs != null)
            {
                foreach (var evtId in dto.EventIDs)
                {
                    var evt = await _eventRepo.GetByIdAsync(evtId);
                    if (evt != null) service.Events.Add(evt);
                }
            }

            // 3. Update Images (SMART LOGIC)

            // A. பழைய படங்கள் எதை Frontend-ல் நீக்கினார்களோ அதை இங்கேயும் நீக்கவும்
            // (DTO-வில் ImageUrls என்பது "தக்கவைத்துக்கொள்ள வேண்டிய" பழைய படங்களின் URL பட்டியல்)
            var keptUrls = dto.ImageUrls ?? new List<string>();

            var imagesToDelete = service.ServiceImages
                .Where(img => !keptUrls.Contains(img.ImageUrl))
                .ToList();

            if (imagesToDelete.Any())
            {
                _serviceRepo.DeleteImages(imagesToDelete);
            }

            // B. புதிய படங்களை Upload செய்து சேர்க்கவும்
            if (images != null && images.Any())
            {
                foreach (var file in images)
                {
                    var uploadResult = await _photoService.AddPhotoAsync(file);
                    service.ServiceImages.Add(new ServiceImage
                    {
                        ServiceImageID = Guid.NewGuid(),
                        ImageUrl = uploadResult.SecureUrl.AbsoluteUri,
                        IsCover = false,
                        ServiceItemID = service.ServiceItemID
                    });
                }
            }

            // C. Cover Photo Logic (Optional: Make first item cover)
            var allImages = service.ServiceImages.ToList();
            for (int i = 0; i < allImages.Count; i++) allImages[i].IsCover = (i == 0);

            // 4. Save
            await _serviceRepo.UpdateAsync(service);
        }

        public async Task<ServiceItemDto> GetServiceByIdAsync(Guid serviceId)
        {
            var service = await _serviceRepo.GetByIdAsync(serviceId);
            return ServiceMapper.MapToServiceDto(service);
        }

        public async Task<IEnumerable<ServiceItemDto>> GetAllServicesAsync()
        {
            var services = await _serviceRepo.GetAllAsync();
            return services.Select(ServiceMapper.MapToServiceDto);
        }

        public async Task<IEnumerable<ServiceItemDto>> GetServicesByVendorAsync(Guid vendorId)
        {
            var services = await _serviceRepo.GetByVendorIdAsync(vendorId);
            return services.Select(ServiceMapper.MapToServiceDto);
        }

        public async Task<IEnumerable<ServiceItemDto>> SearchServicesAsync(ServiceSearchDto searchDto)
        {
            // 🚨 இங்கே 'throw new NotImplementedException()' இருக்கக்கூடாது.

            var services = await _serviceRepo.SearchServicesAsync(searchDto);

            // Map Entity to DTO
            return services.Select(ServiceMapper.MapToServiceDto);
        }
        public async Task<bool> ToggleStatusAsync(Guid serviceId)
        {
            var service = await _serviceRepo.GetByIdAsync(serviceId);
            if (service == null) throw new Exception("Service not found");

            // Toggle the status
            service.Active = !service.Active;

            await _serviceRepo.UpdateAsync(service);
            return service.Active;
        }
    }
}