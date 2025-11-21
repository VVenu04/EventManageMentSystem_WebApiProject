using Application.DTOs.Payment;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Application.Services
{
    public class PaymentService: IPaymentService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IAuthRepository _authRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly string _stripeSecretKey;

        public PaymentService(IBookingRepository bookingRepo,
                              IAuthRepository authRepo,
                              IPaymentRepository paymentRepo,
                              IConfiguration config)
        {
            _bookingRepo = bookingRepo;
            _authRepo = authRepo;
            _paymentRepo = paymentRepo;
            _stripeSecretKey = config["StripeSettings:SecretKey"];
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        // 1. Stripe-ல் Payment Intent-ஐ உருவாக்குதல்
        public async Task<string> CreatePaymentIntentAsync(PaymentRequestDto dto)
        {
            var booking = await _bookingRepo.GetByIdAsync(dto.BookingID);
            if (booking == null) throw new Exception("Booking not found");

            var options = new PaymentIntentCreateOptions
            {
                // Stripe சතங்களில் (cents) கேட்கும் (LKR 100 = 10000 cents)
                Amount = (long)(booking.TotalPrice * 100),
                Currency = "lkr",
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "BookingID", booking.BookingID.ToString() }
                }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            return intent.ClientSecret;
        }

        // 2. Payment-ஐ உறுதி செய்து, பணத்தைப் பிரித்தல் (Core Logic)
        public async Task<bool> ConfirmPaymentAndDistributeFundsAsync(string paymentIntentId)
        {
            // A. Stripe-ல் Payment நிலையைச் சரிபார்
            var service = new PaymentIntentService();
            var intent = await service.GetAsync(paymentIntentId);

            if (intent.Status != "succeeded") return false;

            // B. ஏற்கனவே பதிவு செய்யப்பட்டதா எனச் சோதி (Duplicate Check)
            var existingPayment = await _paymentRepo.GetByPaymentIntentIdAsync(paymentIntentId);
            if (existingPayment != null) return true; // ஏற்கனவே முடிந்துவிட்டது

            // C. Booking-ஐ எடு
            if (!intent.Metadata.ContainsKey("BookingID")) return false;
            var bookingId = Guid.Parse(intent.Metadata["BookingID"]);

            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) return false;

            // --- BUSINESS LOGIC: 10% vs 5% Calculation ---

            decimal totalAmount = booking.TotalPrice;
            decimal adminShare = 0;
            decimal vendorShare = 0;
            decimal customerCashback = 0;

            // Package Booking-ஆ எனச் சோதி (PackageID உள்ளதா?)
            bool isPackageBooking = booking.BookingItems.Any(bi => bi.PackageID != null);

            if (isPackageBooking)
            {
                // --- PACKAGE LOGIC ---
                // Admin: 5%, Customer: 5% (Cashback), Vendor: 90%
                adminShare = totalAmount * 0.05m;
                customerCashback = totalAmount * 0.05m;
                vendorShare = totalAmount * 0.90m;
            }
            else
            {
                // --- SINGLE SERVICE LOGIC ---
                // Admin: 10%, Vendor: 90%
                adminShare = totalAmount * 0.10m;
                vendorShare = totalAmount * 0.90m;
                customerCashback = 0;
            }

            // --- D. Save Payment Record (Using Repository) ---
            var payment = new Payment
            {
                PaymentID = Guid.NewGuid(),
                BookingID = bookingId,
                StripePaymentIntentId = paymentIntentId,
                AmountPaid = totalAmount,
                Status = "Succeeded",
                PaymentDate = DateTime.UtcNow,

                // Shares
                AdminCommission = adminShare,
                VendorEarnings = vendorShare,
                CustomerCashback = customerCashback
            };

            await _paymentRepo.AddAsync(payment);

            // --- E. Update Customer Wallet (Cashback இருந்தால்) ---
            if (customerCashback > 0)
            {
                var customer = await _authRepo.GetCustomerByIdAsync(booking.CustomerID);
                if (customer != null)
                {
                    customer.WalletBalance += customerCashback;
                    await _authRepo.UpdateCustomerAsync(customer); // (Repo-வில் இந்த method தேவை)
                }
            }

            // --- F. Update Booking Status ---
            booking.BookingStatus = "Paid";
            await _bookingRepo.UpdateAsync(booking); // (Repo-வில் இந்த method தேவை)

            return true;
        }
    }
}
