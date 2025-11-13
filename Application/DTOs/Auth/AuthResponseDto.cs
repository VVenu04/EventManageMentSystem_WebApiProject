using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string Token { get; set; } // The JWT Token
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
    }
}
