
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
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
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }


        public AuthResponseDto GenerateToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            if (user.CompanyId.HasValue)
                claims.Add(new Claim("companyId", user.CompanyId.Value.ToString()));

            if (user.CompanyBranchId.HasValue)
                claims.Add(new Claim("companyBranchId", user.CompanyBranchId.Value.ToString()));

            var expiry = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
                CompanyId = user.CompanyId,
                CompanyBranchId = user.CompanyBranchId,
                ExpiresAt = expiry
            };
        }
    }
}
