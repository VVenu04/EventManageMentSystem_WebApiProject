using Application.DTOs.Booking;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Mapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IServiceItemRepository _serviceRepo;
        private readonly IAuthRepository _authRepo;
        private readonly IPackageRepository _packageRepo;
        private readonly IConfiguration _configuration;

        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;
        private readonly IPaymentRepository _paymentRepository;

        private readonly List<string> _validStatuses = new List<string>
        {
            "Preparing", "OnTheWay", "Arrived", "ServiceStarted", "JobDone"
        };

        public BookingService(IBookingRepository bookingRepo,
                              IServiceItemRepository serviceRepo,
                              IAuthRepository authRepo,
                              IPackageRepository packageRepo,
                              IPaymentService paymentService,
                              INotificationService notificationService,
                              IConfiguration configuration,
                              IPaymentRepository paymentRepository)
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
            _authRepo = authRepo;
            _packageRepo = packageRepo;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _configuration = configuration;
            _paymentRepository = paymentRepository;
        }

        public async Task<BookingConfirmationDto> CreateBookingAsync(CreateBookingDto createBookingDto, Guid customerId)
        {
            var customer = await _authRepo.GetCustomerByIdAsync(customerId);
            if (customer == null) throw new Exception($"Invalid customer ID: {customerId}. User not found.");

            if (createBookingDto.EventDate.Date < DateTime.UtcNow.Date)
                throw new Exception("Data was expired. Cannot book on a past date.");

            var (servicesToBook, vendorsToBook, totalPrice) = await GetServicesFromDtoAsync(createBookingDto);

            await ValidateBookingLogicAsync(servicesToBook, vendorsToBook, createBookingDto.EventDate);

            var bookingItemsList = servicesToBook.Select(service => new BookingItem
            {
                BookingItemID = Guid.NewGuid(),
                ServiceItemID = service.ServiceItemID,
                VendorID = service.VendorID,
                ItemPrice = service.Price,
                TrackingStatus = "Confirmed",
                CompletionOtp = null
            }).ToList();

            var booking = new Booking
            {
                BookingID = Guid.NewGuid(),
                CustomerID = customerId,
                EventDate = createBookingDto.EventDate,
                EventTime = createBookingDto.EventTime,
                Discription = createBookingDto.Description,
                Location = createBookingDto.Location,
                TotalPrice = totalPrice,
                BookingStatus = "Confirmed",
                BookingItems = bookingItemsList
            };

            await _bookingRepo.AddAsync(booking);

            foreach (var item in bookingItemsList)
            {
                // 👉 UPDATE: Clarified that payment is pending
                string msg = $"New Booking Request! Customer has booked your service for {createBookingDto.EventDate.ToShortDateString()}. (Waiting for Payment)";

                await _notificationService.SendNotificationAsync(
                    item.VendorID,
                    msg,
                    "BookingCreated",
                    booking.BookingID
                );
            }

            await _notificationService.SendNotificationAsync(
                customerId,
                "Booking Successful! Please proceed to payment.",
                "BookingConfirmation",
                booking.BookingID
            );

            return BookingMapper.MapToConfirmationDto(booking, customer, servicesToBook);
        }

        // ... Getters (Unchanged) ...
        public async Task<IEnumerable<BookingConfirmationDto>> GetBookingsByCustomerAsync(Guid customerId)
        {
            var bookings = await _bookingRepo.GetBookingsByCustomerAsync(customerId);
            return bookings.Select(b => BookingMapper.MapToConfirmationDto(b)).ToList();
        }

        public async Task<IEnumerable<BookingConfirmationDto>> GetBookingsByVendorAsync(Guid vendorId)
        {
            var bookings = await _bookingRepo.GetBookingsByVendorAsync(vendorId);

            // We need a list to hold the specific items
            var dtos = new List<BookingConfirmationDto>();

            foreach (var booking in bookings)
            {
                // 1. Filter: Get only the items in this booking that belong to THIS Vendor
                var vendorItems = booking.BookingItems.Where(bi => bi.VendorID == vendorId);

                foreach (var item in vendorItems)
                {
                    // 2. Base Map: Get the Customer & Parent Booking details
                    var dto = BookingMapper.MapToConfirmationDto(booking);

                    // 3. 👉 CRITICAL OVERRIDES: Set the Item-Specific details
                    dto.BookingItemID = item.BookingItemID; // <--- This fixes the 400 Bad Request
                    dto.ServiceName = item.Service?.Name ?? item.Package?.Name ?? "Service";
                    dto.TotalPrice = item.ItemPrice; // Show only what the Vendor earns, not total booking cost
                    dto.BookingStatus = item.TrackingStatus; // Show "Preparing", "JobDone" etc., not "Confirmed"

                    dtos.Add(dto);
                }
            }

            return dtos;
        }

        public async Task<BookingConfirmationDto> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new Exception($"Booking with ID {bookingId} not found.");
            return BookingMapper.MapToConfirmationDto(booking);
        }

        // ... Helpers (Unchanged) ...
        private async Task<(List<ServiceItem> services, Dictionary<Guid, Vendor> vendors, decimal total)> GetServicesFromDtoAsync(CreateBookingDto dto)
        {
            var servicesInCart = new List<ServiceItem>();
            var vendorsInCart = new Dictionary<Guid, Vendor>();
            decimal totalPrice = 0;

            if (dto.PackageID != null && dto.ServiceIDs.Count > 0) throw new Exception("Cannot book both");

            if (dto.PackageID.HasValue)
            {
                var package = await _packageRepo.GetPackageWithServicesAsync(dto.PackageID.Value);
                if (package == null) throw new Exception("Package not found.");
                totalPrice = package.TotalPrice;

                foreach (var packageItem in package.PackageItems)
                {
                    if (packageItem.Service == null) throw new Exception($"Invalid service item in package.");
                    servicesInCart.Add(packageItem.Service);
                    if (!vendorsInCart.ContainsKey(packageItem.Service.VendorID))
                        vendorsInCart.Add(packageItem.Service.VendorID, packageItem.Service.Vendor);
                }
            }
            else if (dto.ServiceIDs != null && dto.ServiceIDs.Any())
            {
                foreach (var serviceId in dto.ServiceIDs)
                {
                    var service = await _serviceRepo.GetByIdAsync(serviceId);
                    if (service == null) throw new Exception($"Service with ID {serviceId} not found.");
                    servicesInCart.Add(service);
                    totalPrice += service.Price;
                    if (!vendorsInCart.ContainsKey(service.VendorID))
                        vendorsInCart.Add(service.VendorID, service.Vendor);
                }
            }
            else
            {
                throw new Exception("Cart is empty.");
            }

            return (servicesInCart, vendorsInCart, totalPrice);
        }

        private async Task ValidateBookingLogicAsync(List<ServiceItem> services, Dictionary<Guid, Vendor> vendors, DateTime eventDate)
        {
            foreach (var service in services)
            {
                if (!service.Active) throw new Exception($"Service '{service.Name}' is unavailable.");
                bool isAlreadyBooked = await _bookingRepo.IsServiceBookedOnDateAsync(service.ServiceItemID, eventDate);
                if (isAlreadyBooked) throw new Exception($"Service '{service.Name}' is already booked on this date.");

                if (service.TimeLimit > 0)
                {
                    if (eventDate.Date < DateTime.UtcNow.AddDays(service.TimeLimit).Date)
                        throw new Exception($"Service '{service.Name}' requires {service.TimeLimit} days lead time.");
                }
                if (service.EventPerDayLimit > 0)
                {
                    int existing = await _bookingRepo.GetBookingCountForServiceOnDateAsync(service.ServiceItemID, eventDate);
                    if (existing >= service.EventPerDayLimit)
                        throw new Exception($"Service '{service.Name}' has reached its daily limit.");
                }
            }
        }

        public async Task CancelBookingAsync(Guid bookingId, Guid customerId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new Exception("Booking not found.");
            if (booking.CustomerID != customerId) throw new Exception("Unauthorized.");
            if (booking.BookingStatus == "Cancelled") throw new Exception("Already cancelled.");

            var deadline = booking.CreatedAt.AddDays(1);
            if (DateTime.UtcNow > deadline) throw new Exception("Cancellation period expired (24h limit).");

            if (booking.BookingStatus == "Paid")
            {
                await _paymentService.RefundPaymentAsync(bookingId);
            }

            booking.BookingStatus = "Cancelled";
            await _bookingRepo.UpdateAsync(booking);

            foreach (var item in booking.BookingItems)
            {
                await _notificationService.SendNotificationAsync(item.VendorID, "Booking Cancelled by customer.", "BookingCancelled", bookingId);
            }
        }

        // =========================================================
        // 👉 CRITICAL FIX: TRACKING STATUS & PAYMENT CHECK
        // =========================================================
        public async Task UpdateTrackingStatusAsync(UpdateTrackingDto dto, Guid vendorId)
        {
            // 1. 👉 FIX: Fetch the BookingItem FIRST to get the correct Parent BookingID
            var item = await _bookingRepo.GetBookingItemByIdAsync(dto.BookingItemID);
            if (item == null) throw new Exception("Booking Item not found.");

            // 2. 👉 FIX: Now check Payment using the Parent BookingID (Not the Item ID)
            var payment = await _paymentRepository.GetByBookingIdAsync(item.BookingID);

            // 3. 👉 FIX: Validate Payment Status
            if (payment == null || payment.Status != "Succeeded")
            {
                throw new Exception("Payment pending! You cannot start the service until the customer pays.");
            }

            // 4. Validate Status
            if (!_validStatuses.Any(s => s.Equals(dto.Status, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception($"Invalid Status. Allowed: {string.Join(", ", _validStatuses)}");
            }

            if (dto.Status == "JobDone")
            {
                string newOtp = new Random().Next(1000, 9999).ToString();
                item.TrackingStatus = "JobDone";
                item.CompletionOtp = newOtp;
                item.OtpExpiry = DateTime.UtcNow.AddMinutes(10);

                await _bookingRepo.UpdateBookingItemAsync(item);

                try
                {
                    var customerEmail = item.Booking?.Customer?.Email;
                    if (!string.IsNullOrEmpty(customerEmail))
                    {
                        SendGmailOtp(customerEmail, newOtp, item.Service?.Name ?? "Service");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Email failed: " + ex.Message);
                }
            }
            else
            {
                item.TrackingStatus = dto.Status;
                await _bookingRepo.UpdateBookingItemAsync(item);

                string displayMsg = GetDisplayMessage(dto.Status, item.Service?.Name);
                await _notificationService.SendNotificationAsync(
                    item.Booking.CustomerID,
                    displayMsg,
                    "TrackingUpdate",
                    item.BookingID
                );
            }
        }

        // ... Helpers (DisplayMsg, Email) ...
        private string GetDisplayMessage(string status, string serviceName)
        {
            return status switch
            {
                "Preparing" => $"Vendor is preparing: {serviceName}",
                "OnTheWay" => $"Vendor is on the way: {serviceName}",
                "Arrived" => "Vendor has arrived.",
                "ServiceStarted" => $"Service started: {serviceName}",
                _ => $"Status: {status}"
            };
        }

        private void SendGmailOtp(string toEmail, string otp, string serviceName)
        {
            var fromEmail = _configuration["EmailSettings:SenderEmail"];
            var appPassword = _configuration["EmailSettings:SenderPassword"];
            var host = _configuration["EmailSettings:SmtpHost"];
            var port = int.Parse(_configuration["EmailSettings:SmtpPort"]);

            var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(fromEmail, appPassword)
            };

            var mailMessage = new MailMessage(fromEmail, toEmail,
                $"Job Completion Code: {serviceName}",
                $@"
                <div style='font-family: Arial; padding: 20px; border: 1px solid #ddd;'>
                    <h2 style='color: #2c3e50;'>Job Completion Request</h2>
                    <p>The vendor has completed: <strong>{serviceName}</strong>.</p>
                    <p>Give them this code to confirm:</p>
                    <h1 style='color: #27ae60;'>{otp}</h1>
                </div>");

            mailMessage.IsBodyHtml = true;
            client.Send(mailMessage);
        }

        // =========================================================
        // 👉 COMPLETE JOB (Final Verification)
        // =========================================================
        public async Task<bool> CompleteServiceAsync(CompleteJobDto dto, Guid vendorId)
        {
            var item = await _bookingRepo.GetBookingItemByIdAsync(dto.BookingItemID);
            if (item == null) throw new Exception("Item not found.");
            if (item.VendorID != vendorId) throw new Exception("Unauthorized.");

            if (item.TrackingStatus != "JobDone") throw new Exception("Mark as 'Job Done' first.");

            if (item.CompletionOtp != dto.Otp) throw new Exception("Invalid OTP.");

            if (item.OtpExpiry < DateTime.UtcNow) throw new Exception("OTP Expired. Request again.");

            item.TrackingStatus = "Completed";
            item.ServiceDate = DateTime.UtcNow;

            await _bookingRepo.UpdateBookingItemAsync(item);

            try
            {
                var customerEmail = item.Booking?.Customer?.Email;
                if (!string.IsNullOrEmpty(customerEmail))
                {
                    SendReceiptEmail(customerEmail, item.Service?.Name ?? "Service", item.ItemPrice);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Receipt failed: " + ex.Message);
            }

            await _notificationService.SendNotificationAsync(
                item.Booking.CustomerID,
                "Service Completed Successfully!",
                "ServiceCompleted",
                item.BookingID
            );

            return true;
        }

        private void SendReceiptEmail(string toEmail, string serviceName, decimal price)
        {
            var fromEmail = _configuration["EmailSettings:SenderEmail"];
            var appPassword = _configuration["EmailSettings:SenderPassword"];
            var host = _configuration["EmailSettings:SmtpHost"];
            var port = int.Parse(_configuration["EmailSettings:SmtpPort"]);

            var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(fromEmail, appPassword)
            };

            var mailMessage = new MailMessage(fromEmail, toEmail,
                $"Receipt: {serviceName}",
                $@"
                <div style='font-family: Arial; padding: 20px; border: 1px solid #ddd;'>
                    <h2 style='color: #27ae60;'>Service Completed</h2>
                    <p><strong>{serviceName}</strong> is done.</p>
                    <p>Paid: ${price}</p>
                </div>");

            mailMessage.IsBodyHtml = true;
            client.Send(mailMessage);
        }
    }
}