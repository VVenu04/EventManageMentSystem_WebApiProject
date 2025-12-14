using Application.DTOs.Payment;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Domain.Constants;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
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
        private readonly INotificationService _notificationService;



        public PaymentService(IBookingRepository bookingRepo,
                              IAuthRepository authRepo,
                              IPaymentRepository paymentRepo,
                              INotificationService notificationService)
                              
        {
            _bookingRepo = bookingRepo;
            _authRepo = authRepo;
            _paymentRepo = paymentRepo;
            _notificationService = notificationService;

        }

        // 1. Stripe-ல் Payment Intent-ஐ உருவாக்குதல்
        public async Task<bool> ProcessMockPaymentAsync(Guid bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new Exception("Booking not found");

            var existingPayment = await _paymentRepo.GetByBookingIdAsync(bookingId);
            if (existingPayment != null && existingPayment.Status == "Succeeded")
                return true; // ஏற்கனவே Payment முடிந்துவிட்டது

            // 3. Customer Wallet சரிபார்ப்பு
            var customer = await _authRepo.GetCustomerByIdAsync(booking.CustomerID);
            if (customer == null) throw new Exception("Customer not found");

            // 4. பணத்தை கணக்கிடுதல் (Wallet vs Total Logic)
            decimal totalAmountToPay = 0; // External Payment (Mock Card)
            decimal walletDeduction = 0;  // Wallet-ல் எடுப்பது

            if (booking.TotalPrice >= customer.WalletBalance)
            {
                // Wallet-ல் பணம் குறைவு. முழு Wallet பணத்தையும் எடுத்துக்கொள்வோம்.
                walletDeduction = customer.WalletBalance;
                totalAmountToPay = booking.TotalPrice - customer.WalletBalance;
            }
            else
            {
                // Wallet-ல் நிறைய பணம் உள்ளது.
                walletDeduction = booking.TotalPrice;
                totalAmountToPay = 0; // வெளியிலிருந்து எதுவும் கட்ட வேண்டாம்
            }

            // 5. Commission Calculation
            decimal startprice = booking.TotalPrice;
            decimal adminShare = 0;
            decimal vendorShare = 0;
            decimal customerCashback = 0;

            bool isPackageBooking = booking.BookingItems.Any(bi => bi.PackageID != null);

            if (isPackageBooking)
            {
                // Package: Admin 5%, Cashback 5%, Vendor 90%
                adminShare = startprice * 0.05m;
                customerCashback = startprice * 0.05m;
                vendorShare = startprice * 0.90m;
            }
            else
            {
                // Single Service: Admin 10%, Vendor 90%
                adminShare = startprice * 0.10m;
                vendorShare = startprice * 0.90m;
                customerCashback = 0;
            }

            // 6. Generate Dummy Transaction ID
            string mockTransactionId = "MOCK_PAY_" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

            // 7. Save Payment Record
            var payment = new Payment
            {
                PaymentID = Guid.NewGuid(),
                BookingID = bookingId,
                TransactionId = mockTransactionId, // Mock ID
                AmountPaid = totalAmountToPay + walletDeduction, // Total Paid
                Status = "Succeeded",
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = totalAmountToPay > 0 ? "MockCard + Wallet" : "Wallet",

                AdminCommission = adminShare,
                VendorEarnings = vendorShare,
                CustomerCashback = customerCashback,
            };

            await _paymentRepo.AddAsync(payment);

            // 8. Update Wallet Balances

            // A. கஸ்டமர் Wallet-ல் பணத்தை கழித்தல் (Used Balance)
            if (walletDeduction > 0)
            {
                customer.WalletBalance -= walletDeduction;
            }

            // B. கஸ்டமருக்கு Cashback கொடுத்தல்
            if (customerCashback > 0)
            {
                customer.WalletBalance += customerCashback;
            }

            // Update Customer once
            await _authRepo.UpdateCustomerAsync(customer);

            // 9. Update Booking Status
            booking.BookingStatus = "Paid";
            await _bookingRepo.UpdateAsync(booking);

            // 👉 NEW LOGIC: SEND NOTIFICATIONS TO VENDORS
            // =========================================================
            if (booking.BookingItems != null)
            {
                foreach (var item in booking.BookingItems)
                {
                    string vendorMsg = $"Payment Received! The customer has paid for Booking #{booking.BookingID.ToString().Substring(0, 6)}. You can now start the job.";

                    await _notificationService.SendNotificationAsync(
                        item.VendorID,
                        vendorMsg,
                        "PaymentConfirmed",
                        booking.BookingID
                    );
                }
            }

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

            
                // 2. Stripe-ல் Refund-ஐ உருவாக்கு
                payment.Status = "Refunded";

                // 2. Wallet Logic (Reverse logic)
                var customer = payment.Booking?.Customer;
                if (customer == null)
                {
                    // Repo include fail ஆனால் தனியாக எடுக்கவும்
                    customer = await _authRepo.GetCustomerByIdAsync(payment.Booking.CustomerID);
                }

                if (customer != null)
                {
                    // A. வாங்கிய Cashback-ஐ திரும்ப எடு
                    if (payment.CustomerCashback > 0)
                    {
                        customer.WalletBalance -= payment.CustomerCashback;
                    }

                    // B. Refund தொகையை Wallet-ல் சேர் (முழு தொகையும் திரும்ப)
                    customer.WalletBalance += payment.AmountPaid;

                    await _authRepo.UpdateCustomerAsync(customer);
                }

                await _paymentRepo.UpdateAsync(payment);
                return true;
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
