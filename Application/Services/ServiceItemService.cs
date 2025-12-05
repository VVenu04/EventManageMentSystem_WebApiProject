using Application.DTOs.Service;
using Application.DTOs.ServiceItem;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Mapper;
using Domain.Entities;
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

        //public ServiceItemService(IServiceItemRepository serviceRepo, ICategoryRepository categoryRepo, IEventRepo eventRepo)
        //{
        //    _serviceRepo = serviceRepo;
        //    _categoryRepo = categoryRepo;
        //    _eventRepo = eventRepo;
        //}

        //public async Task<ServiceItemDto> CreateServiceAsync(CreateServiceDto dto, Guid vendorId)
        //{
        //    if (dto.ImageUrls == null || !dto.ImageUrls.Any())
        //        throw new Exception("You must upload at least one photo for the service.");

        //    if (dto.ImageUrls.Count > 5)
        //        throw new Exception("You cannot add more than 5 photos per service.");

        //    if (await _categoryRepo.GetByIdAsync(dto.CategoryID) == null)
        //        throw new Exception($"Category with ID {dto.CategoryID} not found.");

        //    if (dto.EventID.HasValue && await _eventRepo.GetByIdAsync(dto.EventID.Value) == null)
        //        throw new Exception($"Event with ID {dto.EventID.Value} not found.");

        //    var serviceImages = dto.ImageUrls.Select((url, index) => new ServiceImage
        //    {
        //        ServiceImageID = Guid.NewGuid(),
        //        ImageUrl = url,
        //        IsCover = (index == 0)
        //    }).ToList();

        //    var service = new ServiceItem
        //    {
        //        ServiceItemID = Guid.NewGuid(),
        //        Name = dto.Name,
        //        Description = dto.Description,
        //        Price = dto.Price,
        //        Location = dto.Location,
        //        CategoryID = dto.CategoryID,
        //        EventID = dto.EventID,
        //        EventPerDayLimit = dto.EventPerDayLimit,
        //        TimeLimit = dto.TimeLimit,
        //        VendorID = vendorId,
        //        Active = true,
        //        ServiceImages = serviceImages
        //    };

        //    await _serviceRepo.AddAsync(service);

        //    var fullServiceDetails = await _serviceRepo.GetByIdAsync(service.ServiceItemID);

        //    // இப்போது Map செய்து அனுப்பினால் பெயர்கள் வரும்
        //    return ServiceMapper.MapToServiceDto(fullServiceDetails);
        //}
        public async Task<ServiceItemDto> CreateServiceAsync(CreateServiceDto dto, Guid vendorId)
        {
            // 1. Image Validation
            if (dto.ImageUrls == null || !dto.ImageUrls.Any())
                throw new Exception("You must upload at least one photo for the service.");

            if (dto.ImageUrls.Count > 5)
                throw new Exception("You cannot add more than 5 photos per service.");

            // 2. Category Validation
            var category = await _categoryRepo.GetByIdAsync(dto.CategoryID);
            if (category == null)
                throw new Exception($"Category with ID {dto.CategoryID} not found.");

            // 3. Events Validation & Fetching (புதிய மாற்றம்)
            // கொடுக்கப்பட்ட அத்தனை Event ID-களும் Database-ல் இருக்கிறதா என எடுத்து வருகிறோம்.
            // குறிப்பு: _context.Events அல்லது _eventRepo.GetByIdsAsync போன்ற logic தேவை.
            // நீங்கள் Repo Pattern பயன்படுத்துவதால், இதைச் செய்ய ஒரு வழி தேவை.
            // எளிய வழி: _eventRepo-ல் 'GetEventsByIdsAsync' என்று ஒரு method எழுதி அதை அழைக்கலாம். 
            // அல்லது Loop போட்டு எடுக்கலாம் (சிறிய எண்ணிக்கைக்கு ஓகே).

            var selectedEvents = new List<Event>();
            if (dto.EventIDs != null && dto.EventIDs.Any())
            {
                foreach (var evtId in dto.EventIDs)
                {
                    var evt = await _eventRepo.GetByIdAsync(evtId);
                    if (evt != null)
                    {
                        selectedEvents.Add(evt);
                    }
                }

                // Optional: ஏதாவது ஒரு Event கூட இல்லை என்றால் Error எறியலாம்
                if (selectedEvents.Count == 0)
                    throw new Exception("Invalid Event IDs provided.");
            }

            // 4. Create Service Images List
            var serviceImages = dto.ImageUrls.Select((url, index) => new ServiceImage
            {
                ServiceImageID = Guid.NewGuid(),
                ImageUrl = url,
                IsCover = (index == 0)
            }).ToList();

            // 5. Create Service Entity
            var service = new ServiceItem
            {
                ServiceItemID = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Location = dto.Location,
                CategoryID = dto.CategoryID,

                // EventID = dto.EventID, // <-- இது இனி தேவை இல்லை
                Events = selectedEvents, // <-- List of Events இங்கே இணைக்கப்படுகிறது

                EventPerDayLimit = dto.EventPerDayLimit,
                TimeLimit = dto.TimeLimit,
                VendorID = vendorId,
                Active = true,
                ServiceImages = serviceImages
            };

            // 6. Save to Database
            await _serviceRepo.AddAsync(service);

            // 7. Fetch Full Details (Includes தேவை)
            // கவனிக்கவும்: GetByIdAsync செய்யும் போது .Include(s => s.Events) அவசியம்!
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