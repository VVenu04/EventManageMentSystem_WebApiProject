using Application.DTOs.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface ICartService
    {
        Task<CartDto> AddToCartAsync(AddToCartDto dto);

        // பயனரின் Cart-ஐப் பார்க்க
        Task<CartDto> GetMyCartAsync(Guid customerId);

        // Cart-ல் இருந்து ஒன்றை நீக்க
        Task RemoveFromCartAsync(Guid bookingItemId);

        // Cart-ஐ Checkout செய்ய (Booking-ஆக மாற்ற)
        Task CheckoutAsync(Guid customerId);
    }
}
