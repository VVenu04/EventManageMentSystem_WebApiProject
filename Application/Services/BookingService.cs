using Application.DTOs.Booking;
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
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IServiceItemRepository _serviceRepo;
        private readonly IAuthRepository _authRepo;
        private readonly IPackageRepository _packageRepo; 

        public BookingService(IBookingRepository bookingRepo,
                              IServiceItemRepository serviceRepo,
                              IAuthRepository authRepo,
                              IPackageRepository packageRepo) 
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
            _authRepo = authRepo;
            _packageRepo = packageRepo;
        }

        public async Task<BookingConfirmationDto> CreateBookingAsync(CreateBookingDto createBookingDto, Guid customerId)
        {
            var customer = await _authRepo.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                // Token-ல் இருந்து வரும் ID தவறாக இருந்தால், அது ஒரு தீவிர பிழை.
                throw new Exception($"Invalid customer ID: {customerId}. User not found.");
            }
            if (createBookingDto.EventDate.Date < DateTime.UtcNow.Date)
            {
                throw new Exception("Data was expired. Cannot book on a past date.");
            }

            var (servicesToBook, vendorsToBook, totalPrice) =
                await GetServicesFromDtoAsync(createBookingDto);

            await ValidateBookingLogicAsync(servicesToBook, vendorsToBook, createBookingDto.EventDate);

            var bookingItemsList = servicesToBook.Select(service => new BookingItem
            {
                BookingItemID = Guid.NewGuid(),
                ServiceItemID = service.ServiceItemID,
                VendorID = service.VendorID,
                ItemPrice = service.Price, 
                TrackingStatus = "Confirmed"
            }).ToList();

            var booking = new Booking
            {
                BookingID = Guid.NewGuid(),
                CustomerID = customerId,
                EventDate = createBookingDto.EventDate,
                Location = createBookingDto.Location, 
                TotalPrice = totalPrice, 
                BookingStatus = "Confirmed",
                BookingItems = bookingItemsList
            };

            // 5. Save to Database
            await _bookingRepo.AddAsync(booking);
            
           
           

            // 7. Return Confirmation DTO (Mapper-ஐப் பயன்படுத்தி)
            return BookingMapper.MapToConfirmationDto(booking, customer, servicesToBook);
        }

        public async Task<BookingConfirmationDto> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) {
                throw new Exception($"Booking with ID {bookingId} not found.");


            }
            // Mapper-ஐப் பயன்படுத்தி DTO-ஆக மாற்று
            return BookingMapper.MapToConfirmationDto(booking);
        }

        // --- Helper Method 1: DTO-வில் இருந்து Services-ஐப் பெறுதல் ---
        private async Task<(List<ServiceItem> services, Dictionary<Guid, Vendor> vendors, decimal total)> GetServicesFromDtoAsync(CreateBookingDto dto)
        {
            var servicesInCart = new List<ServiceItem>();
            var vendorsInCart = new Dictionary<Guid, Vendor>();
            decimal totalPrice = 0;

            if (dto.PackageID.HasValue) // --- Package Booking Logic ---
            {
                var package = await _packageRepo.GetPackageWithServicesAsync(dto.PackageID.Value);
                if (package == null)
                    throw new Exception("Package not found.");

                totalPrice = package.TotalPrice; // Package-இன் மொத்த விலை

                foreach (var packageItem in package.PackageItems)
                {
                    if (packageItem.Service == null)
                        throw new Exception($"Package (ID: {package.PackageID}) contains an invalid service item.");

                    servicesInCart.Add(packageItem.Service);

                    if (!vendorsInCart.ContainsKey(packageItem.Service.VendorID))
                    {
                        if (packageItem.Service.Vendor == null)
                            throw new Exception($"Service '{packageItem.Service.Name}' in package has no associated vendor.");

                        vendorsInCart.Add(packageItem.Service.VendorID, packageItem.Service.Vendor);
                    }
                }
            }
            else if (dto.ServiceIDs != null && dto.ServiceIDs.Any()) // --- Individual Service Booking Logic ---
            {
                foreach (var serviceId in dto.ServiceIDs)
                {
                    var service = await _serviceRepo.GetByIdAsync(serviceId);
                    if (service == null)
                        throw new Exception($"Service with ID {serviceId} not found.");

                    servicesInCart.Add(service);
                    totalPrice += service.Price; // தனித்தனியாக விலையைக் கூட்டு

                    if (!vendorsInCart.ContainsKey(service.VendorID))
                    {
                        if (service.Vendor == null)
                            throw new Exception($"Service '{service.Name}' (ID: {service.ServiceItemID}) has no associated vendor.");

                        vendorsInCart.Add(service.VendorID, service.Vendor);
                    }
                }
            }
            else
            {
                throw new Exception("Cart is empty. No services or package selected.");
            }

            return (servicesInCart, vendorsInCart, totalPrice);
        }

        // --- Helper Method 2: Business Logic-ஐச் சோதித்தல் ---
        private async Task ValidateBookingLogicAsync(List<ServiceItem> services, Dictionary<Guid, Vendor> vendors, DateTime eventDate)
        {
            // Loop 1: Services-ஐ Validate செய்
            foreach (var service in services)
            {
                if (!service.Active)
                    throw new Exception($"Sorry, the service '{service.Name}' is currently unavailable.");

                bool isAlreadyBooked = await _bookingRepo.IsServiceBookedOnDateAsync(service.ServiceItemID, eventDate);
                if (isAlreadyBooked)
                    throw new Exception($"Sorry, '{service.Name}' this service {eventDate.ToShortDateString()} booked on this date.");

                if (service.TimeLimit > 0)
                {
                    var leadTimeRequired = DateTime.UtcNow.AddDays(service.TimeLimit);
                    if (eventDate.Date < leadTimeRequired.Date)
                        throw new Exception($"Sorry, the service '{service.Name}' requires booking at least {service.TimeLimit} days in advance.");
                }
                if (service.EventPerDayLimit > 0)
                {
                    int existingBookings = await _bookingRepo.GetBookingCountForServiceOnDateAsync(service.ServiceItemID, eventDate);

                    if (existingBookings >= (int)service.EventPerDayLimit)
                    {
                        throw new Exception($"Sorry, the service '{service.Name}' has reached its booking limit ({(int)service.EventPerDayLimit}) for this date.");
                    }
                }
            }

          
        }
    }
}