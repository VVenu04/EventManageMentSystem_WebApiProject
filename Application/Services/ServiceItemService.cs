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

        public ServiceItemService(
            IServiceItemRepository serviceRepo, ICategoryRepository categoryRepo, IEventRepo eventRepo, IPhotoService photoService)
        {
            _serviceRepo = serviceRepo;
            _categoryRepo = categoryRepo;
            _eventRepo = eventRepo;
            _photoService = photoService;
        }


        public async Task<ServiceItemDto> CreateServiceAsync(CreateServiceDto dto, List<IFormFile> images, Guid vendorId)
        {
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

        public async Task UpdateServiceAsync(Guid serviceId, UpdateServiceDto dto, Guid vendorId)
        {
            // 1. Service-ஐ முழு விவரங்களுடன் எடுக்கவும் (Images & Events)
            var service = await _serviceRepo.GetByIdWithDetailsAsync(serviceId);

            if (service == null) throw new Exception("Service not found");
            if (service.VendorID != vendorId) throw new Exception("Unauthorized");

            // --- A. Update Basic Properties ---
            service.Name = dto.Name;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.Location = dto.Location;
            service.CategoryID = dto.CategoryID;
            service.EventPerDayLimit = dto.EventPerDayLimit;
            service.TimeLimit = dto.TimeLimit;

            // --- B. Update Events (Many-to-Many) ---
            // Events-ஐ Clear செய்துவிட்டு மீண்டும் சேர்ப்பது பாதுகாப்பானது
            service.Events.Clear();
            if (dto.EventIDs != null && dto.EventIDs.Any())
            {
                foreach (var evtId in dto.EventIDs)
                {
                    var evt = await _eventRepo.GetByIdAsync(evtId);
                    if (evt != null) service.Events.Add(evt);
                }
            }

            // --- C. Update Images (SMART LOGIC) 🚨 IMPORTANT ---

            // 1. தற்போது DB-ல் உள்ள படங்கள்
            var existingImages = service.ServiceImages.ToList();

            // 2. Frontend-ல் இருந்து வரும் புதிய லிஸ்ட் (null safety)
            var incomingUrls = dto.ImageUrls ?? new List<string>();

            // 3. DELETE: DB-ல் இருக்கிறது, ஆனால் புதிய லிஸ்டில் இல்லை -> அதை நீக்கு
            var imagesToDelete = existingImages
                .Where(img => !incomingUrls.Contains(img.ImageUrl))
                .ToList();

            if (imagesToDelete.Any())
            {
                // Repository மூலம் நீக்கச் சொல்கிறோம்
                _serviceRepo.DeleteImages(imagesToDelete);
            }

            // 4. ADD: புதிய லிஸ்டில் இருக்கிறது, ஆனால் DB-ல் இல்லை -> அதைச் சேர்
            // (ஏற்கனவே உள்ள படங்களின் URL-ஐ HashSet-ல் எடுப்பது வேகமானது)
            var existingUrlSet = new HashSet<string>(existingImages.Select(i => i.ImageUrl));

            var imagesToAdd = incomingUrls
                .Where(url => !existingUrlSet.Contains(url))
                .ToList();

            foreach (var url in imagesToAdd)
            {
                service.ServiceImages.Add(new ServiceImage
                {
                    ServiceImageID = Guid.NewGuid(),
                    ImageUrl = url,
                    IsCover = false, // பிறகு செட் செய்வோம்
                    ServiceItemID = service.ServiceItemID
                });
            }

            // 5. UPDATE COVER PHOTO
            // லிஸ்டில் உள்ள முதல் படமே எப்போதும் Cover Photo
            var allCurrentImages = service.ServiceImages.ToList(); // புதுப்பிக்கப்பட்ட லிஸ்ட்

            for (int i = 0; i < incomingUrls.Count; i++)
            {
                var url = incomingUrls[i];
                var imgEntity = allCurrentImages.FirstOrDefault(x => x.ImageUrl == url);

                if (imgEntity != null)
                {
                    // முதல் படம் என்றால் True, மற்றவை False
                    imgEntity.IsCover = (i == 0);
                }
            }

            // --- D. Final Save ---
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
    }
}