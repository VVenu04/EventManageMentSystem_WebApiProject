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

        public ServiceItemService(IServiceItemRepository serviceRepo, ICategoryRepository categoryRepo, IEventRepo eventRepo)
        {
           _serviceRepo = serviceRepo;
            _categoryRepo = categoryRepo;
            _eventRepo = eventRepo;
        }

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