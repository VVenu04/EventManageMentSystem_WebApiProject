using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IGenericRepo
{
    public interface IGenericRepo<T> where T : class
    {

        Task<T> AddAsync(T entity);
    }
}
