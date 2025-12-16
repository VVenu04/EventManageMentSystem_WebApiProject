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

        Task<CartDto> GetMyCartAsync(Guid customerId);

        Task RemoveFromCartAsync(Guid bookingItemId);

        Task<Guid> CheckoutAsync(Guid customerId);
    }
}
