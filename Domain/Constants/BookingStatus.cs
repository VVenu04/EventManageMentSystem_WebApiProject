using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Constants
{
    public class BookingStatus
    {
        public const string Cart = "Cart";           // இன்னும் Checkout செய்யவில்லை
        public const string Pending = "Pending";     // Checkout முடிந்தது, Payment பாக்கி
        public const string Confirmed = "Confirmed"; // Payment முடிந்தது
        public const string Cancelled = "Cancelled";
    }
}
