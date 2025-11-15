using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IRepo
{
    public interface ICategoryRepository
    {
        Task<Service> GetByIdAsync(Guid categoryId);

    }
}
