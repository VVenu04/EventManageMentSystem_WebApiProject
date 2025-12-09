using Application.DTOs.Admin;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Mapper;
using Google.GenAI.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AdminService : IAdminService
    {

        private readonly IAdminRepo _adminRepository;
        private readonly IPaymentRepository _paymentRepo;
        public AdminService(IAdminRepo adminRepository, IPaymentRepository paymentRepo)
        {
            _adminRepository = adminRepository;
            _paymentRepo = paymentRepo;
        }
        public async Task<AdminDto> AddAdminAsync(AdminDto adminDTO)
        {
            if (adminDTO == null)
            {
                throw new ArgumentNullException(nameof(adminDTO));
            }
            var admin = AdminMapper.MapToAdmin(adminDTO);
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminDTO.AdminPassword);
            var addedAdmin = await _adminRepository.AddAsync(admin);
            return AdminMapper.MapToAdminDTO(addedAdmin);


        }

        public async Task DeleteAdminAsync(Guid? id)
        {
            if (id == null || id.Value == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var admin = await _adminRepository.GetByIdAsync(x => x.AdminID == id.Value);

            if (admin != null)
            {
                await _adminRepository.DeleteAsync(admin);
            }
        }

        public async Task<AdminDto> GetAdminAsync(Guid adminId)
        {
            if (adminId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(adminId));
            }

            var admin = await _adminRepository.GetByIdAsync(x => x.AdminID == adminId);
            return AdminMapper.MapToAdminDTO(admin);
        }

        public async Task<IEnumerable<AdminDto>> GetAllAsync()
        {
            var Admins = await _adminRepository.GetAllAsync();
            return AdminMapper.MapToAdminDTOList(Admins);
        }
        public async Task<AdminDashboardDto> GetDashboardStatsAsync()
        {
            return await _adminRepository.GetDashboardStatsAsync();
        }

        public async Task<SystemSettingsDto> GetSystemSettingsAsync()
        {
            var settings = await _adminRepository.GetSystemSettingsAsync();
            return new SystemSettingsDto
            {
                SiteName = settings.SiteName,
                SupportEmail = settings.SupportEmail,
                SupportPhone = settings.SupportPhone,
                OfficeAddress = settings.OfficeAddress,
                ServiceCommission = settings.ServiceCommission,
                PackageCommission = settings.PackageCommission,
                CustomerCashback = settings.CustomerCashback,
                MaintenanceMode = settings.MaintenanceMode
            };
        }

        public async Task<SystemSettingsDto> UpdateSystemSettingsAsync(SystemSettingsDto dto)
        {
            var settings = await _adminRepository.GetSystemSettingsAsync();

            // Map DTO to Entity
            settings.SiteName = dto.SiteName;
            settings.SupportEmail = dto.SupportEmail;
            settings.SupportPhone = dto.SupportPhone;
            settings.OfficeAddress = dto.OfficeAddress;
            settings.ServiceCommission = dto.ServiceCommission;
            settings.PackageCommission = dto.PackageCommission;
            settings.CustomerCashback = dto.CustomerCashback;
            settings.MaintenanceMode = dto.MaintenanceMode;

            await _adminRepository.UpdateSystemSettingsAsync(settings);
            return dto;
        }

        // 🚨 FIX: Implementing Change Password Method (விடுபட்ட மெதட்)
        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new Exception("Passwords do not match.");

            var admin = await _adminRepository.GetAdminByIdAsync(userId);
            if (admin == null) throw new Exception("Admin not found");

            // பழைய பாஸ்வேர்ட் சரியா எனப் பார்க்கவும்
            // (In Real App: Use Hashing here)
            if (admin.PasswordHash != dto.CurrentPassword)
                throw new Exception("Incorrect current password.");

            // புதிய பாஸ்வேர்டை செட் செய்யவும்
            admin.PasswordHash = dto.NewPassword;

            // 🚨 Update & Save
            await _adminRepository.UpdateAdminAsync(admin);

            return true;
        }

        // 🚨 FIX: Implementing Transactions List (Optional but recommended for dashboard)
        public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync()
        {
            // Ensure IPaymentRepository has GetAllPaymentsWithDetailsAsync
            var payments = await _paymentRepo.GetAllPaymentsWithDetailsAsync();

            return payments.Select(p => new TransactionDto
            {
                PaymentID = p.PaymentID,
                TransactionID = p.StripePaymentIntentId,
                BookingID = p.BookingID,
                CustomerName = p.Booking?.Customer?.Name ?? "Unknown",
                CustomerEmail = p.Booking?.Customer?.Email ?? "N/A",
                TotalAmount = p.AmountPaid,
                AdminCommission = p.AdminCommission,
                VendorEarnings = p.VendorEarnings,
                Status = p.Status,
                PaymentDate = p.PaymentDate
            }).ToList();
        }




















        // site settings 
       
    }
    
}
