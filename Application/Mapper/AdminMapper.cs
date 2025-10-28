using Application.DTOs;
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
            admin.AdminPassword = dto.AdminPassword;
            admin.AdminEmail = dto.AdminEmail;
            admin.AdminName = dto.AdminName;
            return admin;
        }
        public static AdminDto MapToAdminDTO(Admin admin)
        {
            if (admin == null) return null;
            AdminDto adminDTO = new AdminDto();
            adminDTO.AdminPassword = admin.AdminPassword;
            adminDTO.AdminEmail = admin.AdminEmail;
            adminDTO.AdminName = admin.AdminName;
            return adminDTO;
        }
    }
}
