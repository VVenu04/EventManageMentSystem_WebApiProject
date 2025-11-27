using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Cart
{
    public class CartDto
    {
        public Guid BookingID { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CartItemDto> Items { get; set; }
    }
    public class CartItemDto
    {
        public Guid BookingItemID { get; set; }
        public string Name { get; set; } // Service or Package Name
        public decimal Price { get; set; }
        public string Type { get; set; } // "Service" or "Package"
    }
}
