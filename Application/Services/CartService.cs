using Application.DTOs.Cart;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Domain.Constants;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CartService: ICartService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IServiceItemRepository _serviceRepo;
        private readonly IPackageRepository _packageRepo;
        // (DbSet-ஐப் பயன்படுத்த Context தேவைப்படலாம், அல்லது Repo-ல் AddItem method வேண்டும்)
        // எளிமைக்காக Repo-வில் தேவையான methods இருப்பதாக வைத்துக்கொள்வோம்.

        public CartService(IBookingRepository bookingRepo,
                           IServiceItemRepository serviceRepo,
                           IPackageRepository packageRepo)
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
            _packageRepo = packageRepo;
        }

        public async Task<CartDto> AddToCartAsync(AddToCartDto dto)
        {
            // 1. பயனருக்கு ஏற்கனவே "Cart" உள்ளதா எனப் பார்
            // (GetCartByCustomerIdAsync என்ற method-ஐ Repo-ல் சேர்க்க வேண்டும்)
            var cart = await _bookingRepo.GetCartByCustomerIdAsync(dto.CustomerID);

            if (cart == null)
            {
                // புதிய Cart (Booking) உருவாக்கு
                cart = new Booking
                {
                    BookingID = Guid.NewGuid(),
                    CustomerID = dto.CustomerID,
                    BookingStatus = BookingStatus.Cart, // "Cart" நிலை
                    EventDate = dto.EventDate,
                    Location = dto.Location,
                    EventTime=dto.EventTime,
                    Discription=dto.Description,
                    CreatedAt = DateTime.UtcNow,
                    BookingItems = new List<BookingItem>()
                };
                await _bookingRepo.AddAsync(cart);
            }

            // 2. Item-ஐ உருவாக்கு (Service or Package)
            var newItem = new BookingItem
            {
                BookingItemID = Guid.NewGuid(),
                BookingID = cart.BookingID,
                TrackingStatus = "In Cart"
            };

            decimal priceToAdd = 0;
            string itemName = "";

            if (dto.ServiceID.HasValue)
            {
                var service = await _serviceRepo.GetByIdAsync(dto.ServiceID.Value);
                if (service == null) throw new Exception("Service not found");

                newItem.ServiceItemID = service.ServiceItemID;
                newItem.VendorID = service.VendorID;
                newItem.ItemPrice = service.Price;
                priceToAdd = service.Price;
                itemName = service.Name;
            }
            else if (dto.PackageID.HasValue)
            {
                var package = await _packageRepo.GetPackageWithServicesAsync(dto.PackageID.Value);
                if (package == null) throw new Exception("Package not found");

                newItem.PackageID = package.PackageID;
                newItem.VendorID = package.VendorID;
                newItem.ItemPrice = package.TotalPrice;
                priceToAdd = package.TotalPrice;
                itemName = package.Name;
            }
            else
            {
                throw new Exception("Must provide either ServiceID or PackageID");
            }

            // 3. Item-ஐச் சேர் மற்றும் Update செய்
            // (Repo-வில் AddItemAsync இருந்தால் நல்லது, அல்லது Collection-ல் சேர்த்து Update)
            // இங்கு நேரடியாகச் சேர்ப்பதாக வைத்துக்கொள்வோம் (Repo update தேவை)
            await _bookingRepo.AddItemToCartAsync(newItem);

            // Total Price Update
            cart.TotalPrice += priceToAdd;
            await _bookingRepo.UpdateAsync(cart);

            return await GetMyCartAsync(dto.CustomerID);
        }

        public async Task<CartDto> GetMyCartAsync(Guid customerId)
        {
            var cart = await _bookingRepo.GetCartByCustomerIdAsync(customerId);
            if (cart == null) return new CartDto { Items = new List<CartItemDto>() };

            return new CartDto
            {
                BookingID = cart.BookingID,
                TotalPrice = cart.TotalPrice,
                Items = cart.BookingItems.Select(i => new CartItemDto
                {
                    BookingItemID = i.BookingItemID,
                    Name = i.Service?.Name ?? i.Package?.Name ?? "Unknown",
                    Price = i.ItemPrice,
                    Type = i.ServiceItemID != Guid.Empty ? "Service" : "Package"
                }).ToList()
            };
        }

        public async Task RemoveFromCartAsync(Guid bookingItemId)
        {
            // Repo method தேவை: DeleteItemAsync
            await _bookingRepo.RemoveItemFromCartAsync(bookingItemId);

            // TotalPrice-ஐ update செய்ய மறக்காதீர்கள் (Logic Repo-வில் அல்லது இங்கேயே எழுதலாம்)
        }

        public async Task<Guid> CheckoutAsync(Guid customerId)
        {
            var cart = await _bookingRepo.GetCartByCustomerIdAsync(customerId);

            if (cart == null) throw new Exception("Cart is empty or not found.");

            // Status-ஐ "Pending" என மாற்று
            cart.BookingStatus = BookingStatus.Pending;

            await _bookingRepo.UpdateAsync(cart);

            // 🚨 FIX: Return the BookingID
            return cart.BookingID;
        }

    }
}
