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
    public class VendorService : IVendorService
    {
        private readonly IVendorRepo _vendorRepo;
        public VendorService(IVendorRepo vendorRepo)
        {
            _vendorRepo = vendorRepo;
        }
        public Task<VendorDto> AddVendorAsync(VendorDto vendorDTO)
        {
            throw new NotImplementedException();
        }

        public Task DeleteVendorAsync(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<VendorDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<VendorDto> GetVendorAsync(Guid vendorId)
        {
            throw new NotImplementedException();
        }
    }
}
