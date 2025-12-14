using Application.DTOs.Admin;
using Application.DTOs.AI;
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
        public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync()
        {
            var payments = await _paymentRepo.GetAllPaymentsWithDetailsAsync();

            return payments.Select(p => new TransactionDto
            {
                PaymentID = p.PaymentID,
                TransactionID = p.TransactionId,
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
    }
    
}
