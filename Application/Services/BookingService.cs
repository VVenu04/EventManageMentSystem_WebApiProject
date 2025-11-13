using Application.DTOs.Booking;
using Application.Interface;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IAuthRepository _authRepo;

        public BookingService(IBookingRepository bookingRepo, IServiceRepository serviceRepo, IAuthRepository authRepo)
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
            _authRepo = authRepo;
        }

       

            public async Task<BookingConfirmationDto> CreateBookingAsync(CreateBookingDto createBookingDto, Guid customerId)
        {
            var servicesInCart = new List<Service>();
            var vendorsInCart = new Dictionary<Guid, Vendor>();
            decimal totalPrice = 0;
            var bookingItemsList = new List<BookingItem>();

            // --- 0. Basic Validation ---
            if (createBookingDto.ServiceIDs == null || !createBookingDto.ServiceIDs.Any())
            {
                throw new Exception("Cart is empty. No services to book.");
            }

            if (createBookingDto.EventDate.Date < DateTime.UtcNow.Date)
            {
                throw new Exception("Data was expired. Cannot book on a past date.");
            }

            // --- 1. Loop 1: Validate ALL Services (Logic 1) ---
            foreach (var serviceId in createBookingDto.ServiceIDs)
            {
                var service = await _serviceRepo.GetByIdAsync(serviceId);
                if (service == null)
                {
                    throw new Exception($"Service with ID {serviceId} not found.");
                }

                // *** FIX: The 'Active Check' logic MUST go INSIDE the loop ***
                 if (!service.Active) // [cite: 997]
                {
                    throw new Exception($"Sorry, the service '{service.Name}' is currently unavailable.");
                }
                // *** End of FIX ***

                // Check for double booking
                bool isAlreadyBooked = await _bookingRepo.IsServiceBookedOnDateAsync(serviceId, createBookingDto.EventDate);
                if (isAlreadyBooked)
                {
                    throw new Exception($"Sorry, '{service.Name}' this service {createBookingDto.EventDate.ToShortDateString()} booked on this date.");
                }

                // Check for service-level time limit
                if (service.TimeLimit > 0)
                {
                    var leadTimeRequired = DateTime.UtcNow.AddDays(service.TimeLimit);
                    if (createBookingDto.EventDate.Date < leadTimeRequired.Date)
                    {
                        throw new Exception($"Sorry, the service '{service.Name}' requires booking at least {service.TimeLimit} days in advance.");
                    }
                }

                // Add its vendor to the dictionary
                if (!vendorsInCart.ContainsKey(service.VendorID))
                {
                    if (service.Vendor == null)
                    {
                        throw new Exception($"Service '{service.Name}' (ID: {service.ServiceID}) has no associated Vendor.");
                    }
                    vendorsInCart.Add(service.VendorID, service.Vendor);
                }

                // --- Add to lists for final booking creation ---
                var bookingItem = new BookingItem
                {
                    BookingItemID = Guid.NewGuid(),
                    ServiceID = service.ServiceID,
                    VendorID = service.VendorID,
                    ItemPrice = service.Price,
                    TrackingStatus = "Confirmed"
                };

                bookingItemsList.Add(bookingItem);
                totalPrice += service.Price;
                servicesInCart.Add(service); // Add the full service object for the final DTO mapping
            } // --- End of Loop 1 ---


            // --- 2. Loop 2: Validate ALL Vendors (Logic 2) ---
            foreach (var vendor in vendorsInCart.Values)
            {
                if (vendor.EventPerDayLimit > 0)
                {
                    int existingBookings = await _bookingRepo.GetBookingCountForVendorOnDateAsync(vendor.VendorID, createBookingDto.EventDate);
                    if (existingBookings + 1 > vendor.EventPerDayLimit)
                    {
                        throw new Exception($"Sorry, Vendor '{vendor.Name}' has reached their booking limit ({vendor.EventPerDayLimit}) for this date.");
                    }
                }
            } // --- End of Loop 2 ---


            // --- 3. Create Booking (Logic 3) ---
            // (All validation has passed)
            var booking = new Booking
            {
                BookingID = Guid.NewGuid(),
                CustomerID = customerId,
                EventDate = createBookingDto.EventDate,
                TotalPrice = totalPrice,
                BookingStatus = "Confirmed",
                BookingItems = bookingItemsList
            };

            // 4. Save to Database
            await _bookingRepo.AddAsync(booking);

            // 5. Get Customer details for the response
            var customer = await _authRepo.GetCustomerByIdAsync(customerId);

            // 6. Return the Confirmation DTO
            return new BookingConfirmationDto
            {
                BookingID = booking.BookingID,
                CustomerID = customerId,
                CustomerName = customer?.Name ?? "Customer",
                EventDate = booking.EventDate,
                TotalPrice = booking.TotalPrice,
                BookingStatus = booking.BookingStatus,
                BookedItems = bookingItemsList.Select(item =>
                {
                    // Find the service details from the list we already built
                    var serviceInCart = servicesInCart.First(s => s.ServiceID == item.ServiceID);
                    return new BookingItemDto
                    {
                        BookingItemID = item.BookingItemID,
                        ServiceName = serviceInCart.Name,
                        ItemPrice = item.ItemPrice,
                        VendorName = serviceInCart.Vendor.Name,
                        TrackingStatus = item.TrackingStatus
                    };
                }).ToList()
            };
        }

        public async Task<BookingConfirmationDto> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) return null;

            // Mapping: Entity -> DTO
            return new BookingConfirmationDto
            {
                BookingID = booking.BookingID,
                CustomerID = booking.CustomerID,
                CustomerName = booking.Customer?.Name,
                EventDate = booking.EventDate,
                TotalPrice = booking.TotalPrice,
                BookingStatus = booking.BookingStatus,
                BookedItems = booking.BookingItems.Select(item => new BookingItemDto
                {
                    BookingItemID = item.BookingItemID,
                    ServiceName = item.Service?.Name,
                    ItemPrice = item.ItemPrice,
                    VendorName = item.Service?.Vendor?.Name,
                    TrackingStatus = item.TrackingStatus
                }).ToList()
            };
        
        }

      
    }
}
