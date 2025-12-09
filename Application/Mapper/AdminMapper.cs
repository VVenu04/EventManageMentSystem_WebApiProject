using Application.DTOs.Admin;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapper
{
    public  class AdminMapper
    {
        public static Admin MapToAdmin(AdminDto dto)
        {
            if (dto == null)
                return null;
            Admin admin = new Admin();
           //admin.PasswordHash = dto.AdminPassword;
            admin.AdminEmail = dto.AdminEmail;
            admin.AdminName = dto.AdminName;
            return admin;
        }
        public static AdminDto MapToAdminDTO(Admin admin)
        {
            if (admin == null) return null;
            AdminDto adminDTO = new AdminDto();
            adminDTO.AdminID = admin.AdminID;
            adminDTO.AdminPassword = null;
            adminDTO.AdminEmail = admin.AdminEmail;
            adminDTO.AdminName = admin.AdminName;
            return adminDTO;
        }
        public static IEnumerable<AdminDto> MapToAdminDTOList(IEnumerable<Admin> admins)
        {
            return admins.Select(a => new AdminDto
            {
                AdminID = a.AdminID,
                AdminPassword = null,
                AdminEmail = a.AdminEmail,
                AdminName = a.AdminName

            }).ToList();


        }
    }
}
