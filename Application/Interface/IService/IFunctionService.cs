using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface IFunctionService
    {
        Task<FunctionDto> AddFunctionAsync(FunctionDto functionDTO);
        Task DeleteFunctionAsync(Guid Id);
        Task<FunctionDto> GetFunctionAsync(Guid FunctionId);
        Task<IEnumerable<FunctionDto>> GetAllAsync();
    }
}
