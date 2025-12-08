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
            // 1. Service-ஐ Images உடன் சேர்த்து எடுக்கிறோம்
            var service = await _serviceRepo.GetByIdWithDetailsAsync(serviceId);

            if (service == null) throw new Exception("Service not found");

            // 2. Authorization Check
            if (service.VendorID != vendorId)
                throw new Exception("Unauthorized: You can only delete your own services.");

            // 3. Package Check
            bool isUsedInPackage = await _serviceRepo.IsServiceInAnyPackageAsync(serviceId);
            if (isUsedInPackage)
                throw new Exception($"Cannot delete '{service.Name}' because it is part of an active Package.");

            // 4. Cloudinary Cleanup (Updated Logic)
            if (service.ServiceImages != null && service.ServiceImages.Any())
            {
                foreach (var img in service.ServiceImages)
                {
                    // URL-ல் இருந்து Public ID-ஐ எடுக்கிறோம்
                    string publicId = GetPublicIdFromUrl(img.ImageUrl);

                    if (!string.IsNullOrEmpty(publicId))
                    {
                        // Comment-ஐ எடுத்துவிட்டு, Public ID-ஐ அனுப்புகிறோம்
                        await _photoService.DeletePhotoAsync(publicId);
                    }
                }
            }

            // 5. Database Delete
            await _serviceRepo.DeleteAsync(service);
        }

        // --- Helper Method (இதை Class-க்கு கீழே தனியாக போடவும்) ---
        private string GetPublicIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            try
            {
                // 1. URL-ஐ URI Format-க்கு மாற்றுகிறோம்
                var uri = new Uri(url);

                // 2. கடைசி பகுதி (Filename) மட்டும் எடுக்கிறோம் (எ.கா: image.jpg)
                string fileName = System.IO.Path.GetFileNameWithoutExtension(uri.LocalPath);

                // குறிப்பு: நீங்கள் Folder வைத்து Upload செய்திருந்தால், Folder பெயரையும் எடுக்க வேண்டி வரும்.
                // இப்போதைக்கு Folder இல்லாமல் செய்தால் இதுவே போதும்.

                return fileName;
            }
            catch
            {
                return null; // URL சரியில்லை என்றால் null அனுப்பும்
            }
        }



        public async Task UpdateServiceAsync(Guid serviceId, UpdateServiceDto dto, List<IFormFile> newImages, Guid vendorId)
        {
            // 1. Service-ஐ படங்கள் மற்றும் Events உடன் எடுக்கிறோம்
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


            // --- B. Update Events (Clear & Add) ---
            service.Events.Clear();
            if (dto.EventIDs != null && dto.EventIDs.Any())
            {
                foreach (var evtId in dto.EventIDs)
                {
                    var evt = await _eventRepo.GetByIdAsync(evtId);
                    if (evt != null) service.Events.Add(evt);
                }
            }

            // --- C. Update Images (The Smart Logic) ---

            // 1. Frontend-ல் மிச்சம் இருக்கும் பழைய படங்கள் (Kept URLs)
            // குறிப்பு: Frontend-ல் ஒரு படத்தை Delete பண்ணினால், அந்த URL இந்த லிஸ்டில் வராது.
            var keptUrls = dto.ImageUrls ?? new List<string>();

            // 2. DB-ல் உள்ள மொத்த படங்கள்
            var dbImages = service.ServiceImages.ToList();

            // 3. DELETE LOGIC: DB-ல் இருக்கு, ஆனால் keptUrls-ல் இல்லை என்றால் -> DELETE
            var imagesToDelete = dbImages
                .Where(img => !keptUrls.Contains(img.ImageUrl))
                .ToList();

            foreach (var img in imagesToDelete)
            {
                // Cloudinary-ல் இருந்து நீக்குகிறோம் (Public ID வைத்து)
                string publicId = GetPublicIdFromUrl(img.ImageUrl);
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _photoService.DeletePhotoAsync(publicId);
                }

                // DB-ல் இருந்து நீக்குகிறோம்
                // குறிப்பு: EF Core தானாகவே இதை Track செய்யும், நாம் Collection-ல் இருந்து Remove செய்தாலே போதும்.
                service.ServiceImages.Remove(img);
            }

            // 4. ADD LOGIC: புதிய Files-ஐ Upload செய்கிறோம்
            if (newImages != null && newImages.Count > 0)
            {
                // மொத்த படங்கள் 5-க்கு மேல் போகிறதா என்று செக் பண்ணலாம்
                if ((service.ServiceImages.Count + newImages.Count) > 5)
                    throw new Exception("Total photos cannot exceed 5.");

                foreach (var file in newImages)
                {
                    var uploadResult = await _photoService.AddPhotoAsync(file);
                    if (uploadResult.Error == null)
                    {
                        service.ServiceImages.Add(new ServiceImage
                        {
                            ServiceImageID = Guid.NewGuid(),
                            ImageUrl = uploadResult.SecureUrl.AbsoluteUri,
                            IsCover = false,
                            ServiceItemID = service.ServiceItemID
                        });
                    }
                }
            }

            // 5. UPDATE COVER PHOTO (Optional)
            // லிஸ்டில் உள்ள முதல் படமே Cover Photo
            var allImages = service.ServiceImages.ToList();
            for (int i = 0; i < allImages.Count; i++)
            {
                allImages[i].IsCover = (i == 0);
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

            var services = await _serviceRepo.SearchServicesAsync(searchDto);

            // Map Entity to DTO
            return services.Select(ServiceMapper.MapToServiceDto);
        }
    }
}