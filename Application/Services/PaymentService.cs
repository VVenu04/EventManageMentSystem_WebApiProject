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
            if (existingPayment != null) return true; //ஏற்கனவே முடிந்துவிட்டது

            // C. Booking-ஐ எடு
            if (!intent.Metadata.ContainsKey("BookingID")) return false;
            var bookingId = Guid.Parse(intent.Metadata["BookingID"]);

            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) return false;
           // var huhu = new    PaymentRequestDto();
          
             

            // --- BUSINESS LOGIC: 10% vs 5% Calculation ---
            var h =     booking.CustomerID;
           var cust = await _authRepo.GetCustomerByIdAsync(h);
            if (cust == null) return false;
            if(booking.TotalPrice >= cust.WalletBalance)
            {
                decimal balance = booking.TotalPrice - cust.WalletBalance;

                decimal totalAmount = balance;
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

                if (customerCashback > 0)
                {
                    var customer = await _authRepo.GetCustomerByIdAsync(booking.CustomerID);
                    if (customer != null)
                    {
                        customer.WalletBalance += customerCashback;
                        await _authRepo.UpdateCustomerAsync(customer); // (Repo-வில் இந்த method தேவை)
                    }
                }

            }
            else 
            {
                decimal balance = cust.WalletBalance - booking.TotalPrice;
                decimal totalAmount = balance;
                decimal adminShare = 0;
                decimal vendorShare = 0;
                decimal customerCashback = 0;
               // decimal vendorEarnings = await _

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
                    CustomerCashback = customerCashback,
                   // VendorEarnings = vendorEarnings
                };

                await _paymentRepo.AddAsync(payment);

                if (customerCashback > 0)
                {
                    var customer = await _authRepo.GetCustomerByIdAsync(booking.CustomerID);
                    if (customer != null)
                    {
                        customer.WalletBalance += customerCashback;
                        await _authRepo.UpdateCustomerAsync(customer); // (Repo-வில் இந்த method தேவை)
                    }
                }
                //if (vendorShare > 0)
                //{
                //    var vendor = await _authRepo.GetVendorByIdAsync(booking.);
                //}


            }


                // --- E. Update Customer Wallet (Cashback இருந்தால்) ---


                // --- F. Update Booking Status ---
                booking.BookingStatus = "Paid";
            await _bookingRepo.UpdateAsync(booking); // (Repo-வில் இந்த method தேவை)

            return true;
        }

        public async Task<bool> RefundPaymentAsync(Guid bookingId)
        {
            // 1. Repo-வை வைத்து Payment-ஐ எடு (OLD: _context.Payments...)
            var payment = await _paymentRepo.GetByBookingIdAsync(bookingId);

            if (payment == null || payment.Status != "Succeeded")
            {
                throw new Exception("No successful payment found for this booking.");
            }

            try
            {
                // 2. Stripe-ல் Refund-ஐ உருவாக்கு
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = payment.StripePaymentIntentId,
                    Reason = RefundReasons.RequestedByCustomer
                };

                var refundService = new RefundService();
                await refundService.CreateAsync(refundOptions);

                // 3. Payment Status-ஐ Update செய்
                payment.Status = "Refunded";

                // 4. Wallet Logic
                if (payment.CustomerCashback > 0)
                {
                    // Repo-வில் Include செய்திருப்பதால் payment.Booking.Customer null ஆக இருக்காது
                    var customer = payment.Booking?.Customer;
                    if (customer != null)
                    {
                        customer.WalletBalance -= payment.CustomerCashback;
                        await _authRepo.UpdateCustomerAsync(customer);
                    }
                    // ஒருவேளை Include வேலை செய்யவில்லை என்றால், _authRepo.GetCustomerByIdAsync-ஐப் பயன்படுத்தலாம்
                }

                // (OLD: await _context.SaveChangesAsync();)
                // 5. Repo-வை வைத்து Save செய்
                await _paymentRepo.UpdateAsync(payment);

                return true;
            }
            catch (StripeException ex)
            {
                throw new Exception($"Stripe Refund Failed: {ex.Message}");
            }
        }
        public async Task<IEnumerable<WalletTransactionDto>> GetCustomerWalletHistoryAsync(Guid customerId)
        {
            var payments = await _paymentRepo.GetByCustomerIdAsync(customerId);
            var history = new List<WalletTransactionDto>();

            foreach (var p in payments)
            {
                // 1. Payment Made (Debit)
                if (p.Status == "Succeeded")
                {
                    history.Add(new WalletTransactionDto
                    {
                        Id = p.PaymentID,
                        Description = $"Payment for Booking #{p.BookingID.ToString().Substring(0, 6)}",
                        Amount = p.AmountPaid,
                        Type = "debit", // Red Color
                        Date = p.PaymentDate,
                        Status = "Paid"
                    });
                }

                // 2. Cashback Received (Credit) - (Optional Logic if you track separately)
                if (p.CustomerCashback > 0)
                {
                    history.Add(new WalletTransactionDto
                    {
                        Id = Guid.NewGuid(), // Virtual ID
                        Description = "Cashback Reward",
                        Amount = p.CustomerCashback,
                        Type = "credit", // Green Color
                        Date = p.PaymentDate,
                        Status = "Added"
                    });
                }

                // 3. Refunds (Credit)
                if (p.Status == "Refunded")
                {
                    history.Add(new WalletTransactionDto
                    {
                        Id = p.PaymentID,
                        Description = $"Refund for Booking #{p.BookingID.ToString().Substring(0, 6)}",
                        Amount = p.AmountPaid, // Full or partial
                        Type = "credit",
                        Date = p.PaymentDate, // Or UpdateDate
                        Status = "Refunded"
                    });
                }
            }

            return history.OrderByDescending(h => h.Date);
        }
    }
}
