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
        public static Domain.Entities.ServiceItem MapToService(FunctionDto dto)
        {
            Domain.Entities.ServiceItem service = new Domain.Entities.ServiceItem();
            service.Price = dto.Price;
            service.Description = dto.Description;
            return service;
        }
    }
}
