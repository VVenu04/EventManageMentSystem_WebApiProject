using Application.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface IAdminService
    {
        Task<AdminDto> AddAdminAsync(AdminDto adminDTO);
        Task DeleteAdminAsync(Guid? Id);
        Task<AdminDto> GetAdminAsync(Guid adminId);
        Task<IEnumerable<AdminDto>> GetAllAsync();
        Task<AdminDashboardDto> GetDashboardStatsAsync();
        Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync();
        Task<SystemSettingsDto> GetSystemSettingsAsync();
        
        // 🚨 FIX: இந்த மெதட் விடுபட்டிருந்தது, இதைச் சேர்க்கவும்
        Task<SystemSettingsDto> UpdateSystemSettingsAsync(SystemSettingsDto dto);

        // Change Password
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
        
    }
}
