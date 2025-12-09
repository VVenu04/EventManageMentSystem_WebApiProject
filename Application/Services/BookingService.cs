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
using Microsoft.Extensions.Configuration; // To read appsettings


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
        // 1. Valid Status List (Vendor can only choose these)
        private readonly List<string> _validStatuses = new List<string>
        {
            "Preparing",
            "OnTheWay",
            "Arrived",
            "ServiceStarted",
            "JobDone" // This triggers the OTP
        };
        public BookingService(IBookingRepository bookingRepo,
                              IServiceItemRepository serviceRepo,
                              IAuthRepository authRepo,
                              IPackageRepository packageRepo,
                              IPaymentService paymentService,
                              INotificationService notificationService,
                              IConfiguration configuration) 
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
            _authRepo = authRepo;
            _packageRepo = packageRepo;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _configuration = configuration;
        }

        public async Task<BookingConfirmationDto> CreateBookingAsync(CreateBookingDto createBookingDto, Guid customerId)
        {
            var customer = await _authRepo.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                // Token-ல் இருந்து வரும் ID தவறாக இருந்தால், அது ஒரு தீவிர பிழை.
                throw new Exception($"Invalid customer ID: {customerId}. User not found.");
            }
            if (createBookingDto.EventDate.Date < DateTime.UtcNow.Date)
            {
                throw new Exception("Data was expired. Cannot book on a past date.");
            }

            var (servicesToBook, vendorsToBook, totalPrice) =
                await GetServicesFromDtoAsync(createBookingDto);

            await ValidateBookingLogicAsync(servicesToBook, vendorsToBook, createBookingDto.EventDate);

            var bookingItemsList = servicesToBook.Select(service => new BookingItem
            {
                BookingItemID = Guid.NewGuid(),
                ServiceItemID = service.ServiceItemID,
                VendorID = service.VendorID,
                ItemPrice = service.Price, 
                TrackingStatus = "Confirmed",
                // It should be null initially
                CompletionOtp = null
            }).ToList();

            var booking = new Booking
            {
                BookingID = Guid.NewGuid(),
                CustomerID = customerId,
                EventDate = createBookingDto.EventDate,
                Location = createBookingDto.Location, 
                TotalPrice = totalPrice, 
                BookingStatus = "Confirmed",
                BookingItems = bookingItemsList
            };

            // 5. Save to Database
            await _bookingRepo.AddAsync(booking);

            foreach (var item in bookingItemsList)
            {
                // ஒவ்வொரு Vendor-க்கும் தனித்தனி Notification
                string msg = $"New Booking! Customer has booked your service on {createBookingDto.EventDate.ToShortDateString()}.";

                await _notificationService.SendNotificationAsync(
                    item.VendorID,
                    msg,
                    "BookingCreated",
                    booking.BookingID
                );
            }

            // Customer-க்கும் அனுப்பலாம்
            await _notificationService.SendNotificationAsync(
                customerId,
                "Booking Successful! Please proceed to payment.",
                "BookingConfirmation",
                booking.BookingID
            );

            


            // 7. Return Confirmation DTO (Mapper-ஐப் பயன்படுத்தி)
            return BookingMapper.MapToConfirmationDto(booking, customer, servicesToBook);
        }

        public async Task<IEnumerable<BookingConfirmationDto>> GetBookingsByCustomerAsync(Guid customerId)
        {
            // Repo-வில் இந்த மெதட் இருக்க வேண்டும்
            var bookings = await _bookingRepo.GetBookingsByCustomerAsync(customerId);
            return bookings.Select(b => BookingMapper.MapToConfirmationDto(b)).ToList();
        }

        public async Task<IEnumerable<BookingConfirmationDto>> GetBookingsByVendorAsync(Guid vendorId)
        {
            var bookings = await _bookingRepo.GetBookingsByVendorAsync(vendorId);

            // ஏற்கனவே உள்ள Mapper-ஐப் பயன்படுத்தி List-ஐ மாற்றுகிறோம்
            return bookings.Select(b => BookingMapper.MapToConfirmationDto(b)).ToList();
        }
        public async Task<BookingConfirmationDto> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) {
                throw new Exception($"Booking with ID {bookingId} not found.");


            }
            // Mapper-ஐப் பயன்படுத்தி DTO-ஆக மாற்று
            return BookingMapper.MapToConfirmationDto(booking);
        }

        // --- Helper Method 1: DTO-வில் இருந்து Services-ஐப் பெறுதல் ---
        private async Task<(List<ServiceItem> services, Dictionary<Guid, Vendor> vendors, decimal total)> GetServicesFromDtoAsync(CreateBookingDto dto)
        {
            var servicesInCart = new List<ServiceItem>();
            var vendorsInCart = new Dictionary<Guid, Vendor>();
            decimal totalPrice = 0;

            if (dto.PackageID.HasValue) // --- Package Booking Logic ---
            {
                var package = await _packageRepo.GetPackageWithServicesAsync(dto.PackageID.Value);
                if (package == null)
                    throw new Exception("Package not found.");

                totalPrice = package.TotalPrice; // Package-இன் மொத்த விலை

                foreach (var packageItem in package.PackageItems)
                {
                    if (packageItem.Service == null)
                        throw new Exception($"Package (ID: {package.PackageID}) contains an invalid service item.");

                    servicesInCart.Add(packageItem.Service);

                    if (!vendorsInCart.ContainsKey(packageItem.Service.VendorID))
                    {
                        if (packageItem.Service.Vendor == null)
                            throw new Exception($"Service '{packageItem.Service.Name}' in package has no associated vendor.");

                        vendorsInCart.Add(packageItem.Service.VendorID, packageItem.Service.Vendor);
                    }
                }
            }
            else if (dto.ServiceIDs != null && dto.ServiceIDs.Any()) // --- Individual Service Booking Logic ---
            {
                foreach (var serviceId in dto.ServiceIDs)
                {
                    var service = await _serviceRepo.GetByIdAsync(serviceId);
                    if (service == null)
                        throw new Exception($"Service with ID {serviceId} not found.");

                    servicesInCart.Add(service);
                    totalPrice += service.Price; // தனித்தனியாக விலையைக் கூட்டு

                    if (!vendorsInCart.ContainsKey(service.VendorID))
                    {
                        if (service.Vendor == null)
                            throw new Exception($"Service '{service.Name}' (ID: {service.ServiceItemID}) has no associated vendor.");

                        vendorsInCart.Add(service.VendorID, service.Vendor);
                    }
                }
            }
            else
            {
                throw new Exception("Cart is empty. No services or package selected.");
            }

            return (servicesInCart, vendorsInCart, totalPrice);
        }

        // --- Helper Method 2: Business Logic-ஐச் சோதித்தல் ---
        private async Task ValidateBookingLogicAsync(List<ServiceItem> services, Dictionary<Guid, Vendor> vendors, DateTime eventDate)
        {
            // Loop 1: Services-ஐ Validate செய்
            foreach (var service in services)
            {
                if (!service.Active)
                    throw new Exception($"Sorry, the service '{service.Name}' is currently unavailable.");

                bool isAlreadyBooked = await _bookingRepo.IsServiceBookedOnDateAsync(service.ServiceItemID, eventDate);
                if (isAlreadyBooked)
                    throw new Exception($"Sorry, '{service.Name}' this service {eventDate.ToShortDateString()} booked on this date.");

                if (service.TimeLimit > 0)
                {
                    var leadTimeRequired = DateTime.UtcNow.AddDays(service.TimeLimit);
                    if (eventDate.Date < leadTimeRequired.Date)
                        throw new Exception($"Sorry, the service '{service.Name}' requires booking at least {service.TimeLimit} days in advance.");
                }
                if (service.EventPerDayLimit > 0)
                {
                    int existingBookings = await _bookingRepo.GetBookingCountForServiceOnDateAsync(service.ServiceItemID, eventDate);

                    if (existingBookings >= (int)service.EventPerDayLimit)
                    {
                        throw new Exception($"Sorry, the service '{service.Name}' has reached its booking limit ({(int)service.EventPerDayLimit}) for this date.");
                    }
                }
            }

          
        }

        public async Task CancelBookingAsync(Guid bookingId, Guid customerId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new Exception("Booking not found.");

            // 1. Security Check: இது அந்த Customer-உடைய Booking தானா?
            if (booking.CustomerID != customerId)
            {
                throw new Exception("Unauthorized to cancel this booking.");
            }

            // 2. Status Check: ஏற்கனவே Cancel ஆகிவிட்டதா?
            if (booking.BookingStatus == "Cancelled")
            {
                throw new Exception("Booking is already cancelled.");
            }

            // --- 3. 🚨 TIME LIMIT CHECK (24 Hours Rule) ---
            // Book செய்த நேரம் + 1 நாள்
            var deadline = booking.CreatedAt.AddDays(1);

            if (DateTime.UtcNow > deadline)
            {
                // 20-ம் தேதி Book செய்தால், 21-ம் தேதி தாண்டிவிட்டால் Error வரும்.
                throw new Exception("Cancellation period expired. You can only cancel within 24 hours of booking.");
            }

            // --- 4. Refund Process ---
            // (Booking 'Paid' ஆக இருந்தால் பணத்தைத் திருப்பிக் கொடு)
            if (booking.BookingStatus == "Paid")
            {
                // PaymentService-ஐ Inject செய்ய வேண்டும் (Constructor-ல்)
                await _paymentService.RefundPaymentAsync(bookingId);
            }

            // 5. Update Booking Status
            booking.BookingStatus = "Cancelled";
            await _bookingRepo.UpdateAsync(booking);
            foreach (var item in booking.BookingItems)
            {
                string msg = $"Booking Cancelled. A booking scheduled for {booking.EventDate.ToShortDateString()} has been cancelled by the customer.";

                await _notificationService.SendNotificationAsync(
                    item.VendorID,
                    msg,
                    "BookingCancelled",
                    bookingId
                );
            }
        }



        // UPDATED TRACKING METHOD
        
        public async Task UpdateTrackingStatusAsync(UpdateTrackingDto dto, Guid vendorId)
        {
            // 1. Validate Status Input (Selectable Logic)
            // NEW (Robust - Case Insensitive):
            // Checks if the input exists in the list, ignoring Upper/Lower case differences
            if (!_validStatuses.Any(s => s.Equals(dto.Status, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception($"Invalid Status. Allowed values: {string.Join(", ", _validStatuses)}");
            }

            var item = await _bookingRepo.GetBookingItemByIdAsync(dto.BookingItemID);
            if (item == null) throw new Exception("Item not found.");

            // NEW LOGIC STARTS HERE
            if (dto.Status == "JobDone")
            {
                // 1. Generate NEW OTP
                string newOtp = new Random().Next(1000, 9999).ToString();

                // 2. Save OTP & EXPIRY to Database
                item.TrackingStatus = "JobDone";
                item.CompletionOtp = newOtp;

                // ADD THIS LINE (Sets 10 minute limit):
                item.OtpExpiry = DateTime.UtcNow.AddMinutes(10);

                await _bookingRepo.UpdateBookingItemAsync(item);

                // 3. Send Email using Gmail SMTP (The "Free" solution)
                try
                {
                    // Ensure Customer Email exists
                    var customerEmail = item.Booking?.Customer?.Email; // Ensure your Repo includes Customer data
                    if (!string.IsNullOrEmpty(customerEmail))
                    {
                        SendGmailOtp(customerEmail, newOtp, item.Service?.Name ?? "Service");
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't stop the process
                    Console.WriteLine("Email failed: " + ex.Message);
                }
            }
            else
            {
                // 3. Normal Status Updates (Preparing, OnTheWay, etc.)
                item.TrackingStatus = dto.Status;
                await _bookingRepo.UpdateBookingItemAsync(item);

                // Normal Notification
                string displayMsg = GetDisplayMessage(dto.Status, item.Service?.Name);

                await _notificationService.SendNotificationAsync(
                    item.Booking.CustomerID,
                    displayMsg,
                    "TrackingUpdate",
                    item.BookingID
                );
            }
        }

        // Helper for nice messages
        private string GetDisplayMessage(string status, string serviceName)
        {
            return status switch
            {
                "Preparing" => $"Vendor is preparing your service: {serviceName}.",
                "OnTheWay" => $"Vendor is on the way for: {serviceName}.",
                "Arrived" => $"Vendor has arrived at the location.",
                "ServiceStarted" => $"The service '{serviceName}' has started.",
                _ => $"Status updated to {status}."
            };
        }


        private void SendGmailOtp(string toEmail, string otp, string serviceName)
        {
            // 1. READ FROM YOUR JSON (Matching the exact names you just showed me)
            var fromEmail = _configuration["EmailSettings:SenderEmail"];    // Was "FromEmail"
            var appPassword = _configuration["EmailSettings:SenderPassword"]; // Was "AppPassword"
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
        <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
            <h2 style='color: #2c3e50;'>Job Completion Request</h2>
            <p>The vendor has completed the service: <strong>{serviceName}</strong>.</p>
            <p>To confirm this job is done, please give them this code:</p>
            <h1 style='color: #27ae60; letter-spacing: 5px;'>{otp}</h1>
            <p style='font-size: 12px; color: #888;'>If you did not request this, please ignore this email.</p>
        </div>
        ");

            mailMessage.IsBodyHtml = true;
            client.Send(mailMessage);
        }
        // COMPLETE JOB (OTP Verification)

        public async Task<bool> CompleteServiceAsync(CompleteJobDto dto, Guid vendorId)
        {
            var item = await _bookingRepo.GetBookingItemByIdAsync(dto.BookingItemID);

            if (item == null) throw new Exception("Item not found.");
            if (item.VendorID != vendorId) throw new Exception("Unauthorized.");

            // 1. Check if status is correct (Must be 'JobDone' before completing)
            if (item.TrackingStatus != "JobDone")
            {
                throw new Exception("You must mark the job as 'Job Done' before entering the OTP.");
            }

            // 2. Validate OTP
            if (item.CompletionOtp != dto.Otp)
            {
                throw new Exception("Invalid OTP! Ask the customer for the correct code.");
            }

            //  ADD THIS CHECK (Expiry Logic):
            if (item.OtpExpiry < DateTime.UtcNow)
            {
                throw new Exception("OTP has Expired! Please request the job completion again to get a new code.");
            }


            // 3. Success! Mark as Completed
            item.TrackingStatus = "Completed";
            item.ServiceDate = DateTime.UtcNow;

            // TODO: Trigger Payment Release (e.g., _paymentService.ReleaseFunds...)

            await _bookingRepo.UpdateBookingItemAsync(item);

            // 1. Send the Real Email
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
                Console.WriteLine("Receipt email failed: " + ex.Message);
            }

            // 4. Final Notification
            await _notificationService.SendNotificationAsync(
                item.Booking.CustomerID,
                "Service Completed Successfully! Receipt sent to email.",
                "ServiceCompleted",
                item.BookingID
            );

            return true;
        }

        private void SendReceiptEmail(string toEmail, string serviceName, decimal price)
        {
            // READ CONFIG
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
                $"Receipt: {serviceName} Completed",
                $@"
        <div style='font-family: Arial; padding: 20px; border: 1px solid #ddd;'>
            <h2 style='color: #27ae60;'>Service Completed</h2>
            <p>Your service <strong>{serviceName}</strong> has been successfully finished.</p>
            <hr>
            <p><strong>Amount Paid:</strong> ${price}</p>
            <p><strong>Status:</strong> Completed ✅</p>
            <p>Thank you for using Smart Function!</p>
        </div>
        ");

            mailMessage.IsBodyHtml = true;
            client.Send(mailMessage);
        }

    }
}