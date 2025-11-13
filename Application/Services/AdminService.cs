using Application.DTOs;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Mapper;
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

    }
    
}
