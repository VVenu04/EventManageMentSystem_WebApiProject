using Application.DTOs.Admin;
using Application.DTOs.AI;
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
    }
}
