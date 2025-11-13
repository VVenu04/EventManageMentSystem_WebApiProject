using Application.DTOs.Booking;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Interface;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
            decimal totalPrice = 0;
            var bookingItemsList = new List<BookingItem>();


            foreach (var serviceId in createBookingDto.ServiceIDs)
            {
                var service = await _serviceRepo.GetByIdAsync(serviceId);
                if (service == null)
                {
                    throw new Exception($"Service with ID {serviceId} not found.");
                }


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
            }


            var booking = new Booking
            {
                BookingID = Guid.NewGuid(),
                CustomerID = customerId,
                EventDate = createBookingDto.EventDate,
                TotalPrice = totalPrice,
                BookingStatus = "Confirmed",
                BookingItems = bookingItemsList
            };

            await _bookingRepo.AddAsync(booking);
            var customer = await _authRepo.GetCustomerByIdAsync(customerId);

            return new BookingConfirmationDto
            {
                BookingID = booking.BookingID,
                CustomerID = customerId,
                CustomerName = customer?.Name ?? "Customer",
                EventDate = booking.EventDate,
                TotalPrice = booking.TotalPrice,
                BookingStatus = booking.BookingStatus,
                BookedItems = bookingItemsList.Select(item => new BookingItemDto
                {
                    BookingItemID = item.BookingItemID,
                    ServiceName = (item.Service?.Name ?? "Service"),
                    ItemPrice = item.ItemPrice,
                    VendorName = (item.Service?.Vendor?.Name ?? "Vendor"),
                    TrackingStatus = item.TrackingStatus
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
