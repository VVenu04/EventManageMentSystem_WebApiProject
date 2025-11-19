using Application.DTOs.Booking;
using Domain.Entities; // <-- 1. பிழையைச் சரிசெய்ய, இந்த 'using' வரி மிக முக்கியம்
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mapper // (உங்கள் folder பெயர் 'Mappers' எனில்)
{
    public static class BookingMapper
    {
        // GetBookingByIdAsync-ஆல் call செய்யப்படும்
        public static BookingConfirmationDto? MapToConfirmationDto(Booking booking)
        {
            if (booking == null) return null;

            return new BookingConfirmationDto
            {
                BookingID = booking.BookingID,
                CustomerID = booking.CustomerID,
                CustomerName = booking.Customer?.Name,
                EventDate = booking.EventDate,
                TotalPrice = booking.TotalPrice,
                BookingStatus = booking.BookingStatus,
                // 'using Domain.Entities;' இருப்பதால், 'item' இப்போது 'BookingItem' எனச் சரியாகப் புரியும்
                BookedItems = booking.BookingItems?.Select(item => new BookingItemDto
                {
                    BookingItemID = item.BookingItemID,
                    ServiceName = item.Service?.Name, // <-- பிழை (Error) இருந்த இடம்
                    ItemPrice = item.ItemPrice,
                    VendorName = item.Service?.Vendor?.Name,
                    TrackingStatus = item.TrackingStatus
                }).ToList()
            };
        }

        // CreateBookingAsync-ஆல் call செய்யப்படும்
        public static BookingConfirmationDto MapToConfirmationDto(Booking booking, Customer customer, List<ServiceItem> servicesInCart)
        {
            return new BookingConfirmationDto
            {
                BookingID = booking.BookingID,
                CustomerID = booking.CustomerID,
                CustomerName = customer?.Name ?? "Customer",
                EventDate = booking.EventDate,
                TotalPrice = booking.TotalPrice,
                BookingStatus = booking.BookingStatus,
                BookedItems = booking.BookingItems?.Select(item =>
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