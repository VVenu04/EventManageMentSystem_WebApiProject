using Application.Interface.IGenericRepo;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IRepo
{
    public interface ICategoryRepository : IGenericRepo<Category>
    {

        Task UpdateAsync(Category category);
        Task<Category> GetByIdAsync(Guid id);  // This match the GetByIdAsync(Guid id) method in our IEventRepo

    }
}
