using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IAuth
{
    public interface ITokenService
    {
        string CreateToken(Guid userId, string email, string role);

    }
}
