using Application.DTOs;
using Application.Interface.IRepo;
using Application.Interface.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class FunctionService : IFunctionService
    {
        private readonly IFunctionRepo _repo;
        public FunctionService(IFunctionRepo functionRepo)
        {
            _repo = functionRepo;
        }
        public Task<FunctionDto> AddFunctionAsync(FunctionDto functionDTO)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFunctionAsync(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FunctionDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<FunctionDto> GetFunctionAsync(Guid FunctionId)
        {
            throw new NotImplementedException();
        }
    }
}
