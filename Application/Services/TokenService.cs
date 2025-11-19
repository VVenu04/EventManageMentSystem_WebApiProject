using Application.Interface.IAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TokenService: ITokenService
    {
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration configuration)
        {
            var tokenKey = configuration["TokenKey"] ?? throw new Exception("Token key not found");
            if (tokenKey.Length < 64) throw new Exception("Token key must be at least 64 characters long");
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
        }

        public string CreateToken(Guid userId, string email, string role)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Email, email),
                new(JwtRegisteredClaimNames.NameId, userId.ToString()),
                new(ClaimTypes.Role, role) // *** இது மிக முக்கியம் ***
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
