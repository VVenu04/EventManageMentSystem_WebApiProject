using Application.DTOs.Booking;
using Domain.Entities; 
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mapper
{
    public static class BookingMapper
    {
        public static BookingConfirmationDto MapToConfirmationDto(Booking booking)
        {
            if (booking == null) return null;

            return new BookingConfirmationDto
            {
                BookingID = booking.BookingID,
                CustomerID = booking.CustomerID,
                CustomerName = booking.Customer?.Name,
                EventDate = booking.EventDate,
                EventTime = booking.EventTime,
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

        public static BookingConfirmationDto MapToConfirmationDto(Booking booking, Customer customer, List<ServiceItem> servicesInCart)
        {
            return new BookingConfirmationDto
            {
                BookingID = booking.BookingID,
                CustomerID = booking.CustomerID,
                CustomerName = customer?.Name ?? "Customer",
                EventDate = booking.EventDate,
                EventTime = booking.EventTime,
                TotalPrice = booking.TotalPrice,
                BookingStatus = booking.BookingStatus,
                BookedItems = booking.BookingItems.Select(item =>
                {
                    var serviceInCart = servicesInCart.First(s => s.ServiceItemID == item.ServiceItemID);

                    return new BookingItemDto
                    {
                        BookingItemID = item.BookingItemID,
                        ServiceName = serviceInCart.Name,
                        ItemPrice = item.ItemPrice,
                        VendorName = serviceInCart.Vendor?.Name,
                        TrackingStatus = item.TrackingStatus
                    };
                }).ToList()
            };
        }
    }
}