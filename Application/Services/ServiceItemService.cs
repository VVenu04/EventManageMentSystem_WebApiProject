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

        public async Task UpdateServiceAsync(Guid serviceId, UpdateServiceDto updateServiceDto, Guid vendorId)
        {
            // முக்கியம்: இங்கே Service-ஐ எடுக்கும் போது, அதன் பழைய Events-ஐயும் சேர்த்து (Include) எடுக்க வேண்டும்.
            // உங்கள் Repository-இல் GetByIdAsync உடன் Include போடும் வசதி இல்லையென்றால், 
            // தனியாக 'GetServiceWithEventsAsync' என ஒரு method எழுதுவது நல்லது.
            // எ.கா: var service = await _context.ServiceItems.Include(s => s.Events).Include(s => s.ServiceImages)...

            var service = await _serviceRepo.GetByIdWithDetailsAsync(serviceId); // Include(s => s.Events) அவசியம்!

            if (service == null) throw new Exception("Service not found");

            if (service.VendorID != vendorId)
            {
                throw new Exception("You are not authorized to update this service");
            }

            // --- Validation ---
            if (updateServiceDto.ImageUrls == null || !updateServiceDto.ImageUrls.Any())
                throw new Exception("You must have at least one photo for the service.");

            if (updateServiceDto.ImageUrls.Count > 5)
                throw new Exception("You cannot add more than 5 photos per service.");

            // --- Update Basic Properties ---
            service.Name = updateServiceDto.Name;
            service.Description = updateServiceDto.Description;
            service.Price = updateServiceDto.Price;
            service.Location = updateServiceDto.Location;
            service.CategoryID = updateServiceDto.CategoryID;
            service.EventPerDayLimit = updateServiceDto.EventPerDayLimit;
            service.TimeLimit = updateServiceDto.TimeLimit;

            // --- Update Events (Many-to-Many Logic) ---
            // 1. பழைய Events இணைப்பை துண்டிக்கவும்
            service.Events.Clear();

            // 2. புதிய Events-ஐ தேடி சேர்க்கவும்
            if (updateServiceDto.EventIDs != null && updateServiceDto.EventIDs.Any())
            {
                foreach (var evtId in updateServiceDto.EventIDs)
                {
                    var evt = await _eventRepo.GetByIdAsync(evtId);
                    if (evt != null)
                    {
                        service.Events.Add(evt);
                    }
                }
            }

            // --- Update Images (Existing Logic) ---
            service.ServiceImages.Clear();
            foreach (var (url, index) in updateServiceDto.ImageUrls.Select((url, index) => (url, index)))
            {
                service.ServiceImages.Add(new ServiceImage
                {
                    ServiceImageID = Guid.NewGuid(),
                    ImageUrl = url,
                    IsCover = (index == 0),
                    ServiceItemID = service.ServiceItemID
                });
            }

            // --- Save Changes ---
            await _serviceRepo.UpdateAsync(service);
        }




        public async Task DeleteServiceAsync(Guid serviceId, Guid vendorId)
        {
            var service = await _serviceRepo.GetByIdAsync(serviceId);
            if (service == null) throw new Exception("Service not found");

            if (service.VendorID != vendorId) throw new Exception("Unauthorized");

            if (await _serviceRepo.IsServiceInAnyPackageAsync(serviceId))
                throw new Exception($"Cannot delete '{service.Name}' because it is part of one or more Packages.");

            await _serviceRepo.DeleteAsync(service);
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