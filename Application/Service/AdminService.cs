using Application.DTOs;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class AdminService : IAdminService
    {

        private readonly IAdminRepo _adminRepository;
        public AdminService(IAdminRepo adminRepository)
        {
            _adminRepository = adminRepository;
        }
        public async Task<AdminDto> AddAdminAsync(AdminDto adminDTO)
        {
            if (adminDTO == null)
            {
                throw new ArgumentNullException(nameof(adminDTO));
            }
            var admin = AdminMapper.MapToAdmin(adminDTO);
            var addedAdmin = await _adminRepository.AddAsync(admin);
            return adminDTO;


        }

        public async Task DeleteAdminAsync(Guid Id)
        {
            if (Id == null)
            {
                throw new ArgumentNullException(nameof(Id));
            }
            var admin = await _adminRepository.GetByIdAsync(x => x.AdminID == Id);
            await _adminRepository.DeleteAsync(admin);

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
    }
    
}
