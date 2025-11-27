using Application.DTOs.Service;
using Application.DTOs.ServiceItem;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Mapper;
using Domain.Entities;
using System.Linq;

namespace Application.Services
{
    public class ServiceItemService : IServiceItemService
    {
        private readonly IServiceItemRepository _serviceRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IEventRepo _eventRepo;

        public ServiceItemService(IServiceItemRepository serviceRepo, ICategoryRepository categoryRepo, IEventRepo eventRepo)
        {
            _serviceRepo = serviceRepo;
            _categoryRepo = categoryRepo;
            _eventRepo = eventRepo;
        }

        public async Task<ServiceItemDto> CreateServiceAsync(CreateServiceDto dto, Guid vendorId)
        {
            if (dto.ImageUrls == null || !dto.ImageUrls.Any())
                throw new Exception("You must upload at least one photo for the service.");

            if (dto.ImageUrls.Count > 5)
                throw new Exception("You cannot add more than 5 photos per service.");

            if (await _categoryRepo.GetByIdAsync(dto.CategoryID) == null)
                throw new Exception($"Category with ID {dto.CategoryID} not found.");

            if (dto.EventID.HasValue && await _eventRepo.GetByIdAsync(dto.EventID.Value) == null)
                throw new Exception($"Event with ID {dto.EventID.Value} not found.");

            var serviceImages = dto.ImageUrls.Select((url, index) => new ServiceImage
            {
                ServiceImageID = Guid.NewGuid(),
                ImageUrl = url,
                IsCover = (index == 0) 
            }).ToList();

            var service = new ServiceItem
            {
                ServiceItemID = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Location = dto.Location,
                CategoryID = dto.CategoryID,
                EventID = dto.EventID,
                EventPerDayLimit = dto.EventPerDayLimit,
                TimeLimit = dto.TimeLimit,
                VendorID = vendorId,
                Active = true,
                ServiceImages = serviceImages 
            };

            await _serviceRepo.AddAsync(service);

            var newServiceData = await _serviceRepo.GetByIdAsync(service.ServiceItemID);
            return ServiceMapper.MapToServiceDto(newServiceData);
        }

        public async Task UpdateServiceAsync(Guid serviceId, UpdateServiceDto updateServiceDto, Guid vendorId)
        {
            // 1. Service-ஐ Database-ல் இருந்து எடு (Photos-உடன் சேர்த்து)
            // (GetByIdAsync-ல் Include(s => s.ServiceImages) இருக்க வேண்டும்)
            var service = await _serviceRepo.GetByIdAsync(serviceId);

            if (service == null) throw new Exception("Service not found");

            // 2. Security Check: இது இந்த Vendor-உடையதுதானா?
            if (service.VendorID != vendorId)
            {
                throw new Exception("You are not authorized to update this service");
            }

            // 3. Validation (Photos)
            if (updateServiceDto.ImageUrls == null || !updateServiceDto.ImageUrls.Any())
                throw new Exception("You must have at least one photo for the service.");

            if (updateServiceDto.ImageUrls.Count > 5)
                throw new Exception("You cannot add more than 5 photos per service.");

            // 4. Update simple properties (பெயர், விலை, விபரம்)
            service.Name = updateServiceDto.Name;
            service.Description = updateServiceDto.Description;
            service.Price = updateServiceDto.Price;
            service.Location = updateServiceDto.Location;
            service.CategoryID = updateServiceDto.CategoryID;
            service.EventID = updateServiceDto.EventID;
            service.EventPerDayLimit = updateServiceDto.EventPerDayLimit;
            service.TimeLimit = updateServiceDto.TimeLimit;

            // 5. Update Photos (பழையதை அழித்து புதியதைச் சேர்த்தல்)
            // Frontend-ல் இருந்து வரும் லிஸ்டில் பழைய போட்டோக்களும் இருக்க வேண்டும்.
            service.ServiceImages.Clear();

            foreach (var (url, index) in updateServiceDto.ImageUrls.Select((url, index) => (url, index)))
            {
                service.ServiceImages.Add(new ServiceImage
                {
                    ServiceImageID = Guid.NewGuid(),
                    ImageUrl = url,
                    IsCover = (index == 0) // முதல் போட்டோ Cover ஆக இருக்கும்
                });
            }

            // 6. Save changes via Repo
            await _serviceRepo.UpdateAsync(service);
        }

        public async Task DeleteServiceAsync(Guid serviceId, Guid vendorId)
        {
            var service = await _serviceRepo.GetByIdAsync(serviceId);

            // 1. Service இருக்கிறதா?
            if (service == null)
            {
                throw new Exception("Service not found");
            }

            // 2. இது அந்த Vendor-இன் Service தானா?
            if (service.VendorID != vendorId)
            {
                throw new Exception("You are not authorized to delete this service");
            }

            // --- 3. 🚨 புதிய Logic: Package-ல் உள்ளதா எனச் சோதி ---
            bool isInPackage = await _serviceRepo.IsServiceInAnyPackageAsync(serviceId);

            if (isInPackage)
            {
                // Package-ல் இருந்தால் Error காட்டு (அழிக்காதே)
                throw new Exception($"Cannot delete '{service.Name}' because it is part of one or more Packages. Please remove it from the packages first.");
            }
            // --- ---

            // 4. எல்லாம் சரியாக இருந்தால் Delete செய்
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
            var services = await _serviceRepo.SearchServicesAsync(searchDto);

            // Mapper-ஐப் பயன்படுத்தி DTO-வாக மாற்று
            return services.Select(ServiceMapper.MapToServiceDto);
        }

        Task<IEnumerable<ServiceItem>> IServiceItemService.SearchServicesAsync(ServiceSearchDto searchDto)
        {
            throw new NotImplementedException();
        }

        public Task UpdateServiceAsync(Guid serviceId, CreateServiceDto updateServiceDto, Guid vendorId)
        {
            throw new NotImplementedException();
        }
    }
}
