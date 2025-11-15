using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapper
{
    public class FunctionMapper
    {
        public static Domain.Entities.Service MapToService(FunctionDto dto)
        {
            Domain.Entities.Service service = new Domain.Entities.Service();
            service.Price = dto.Price;
            service.Description = dto.Description;
            return service;
        }
    }
}
