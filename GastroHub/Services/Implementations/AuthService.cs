using GastroHub.Dtos.Auth;
using GastroHub.Dtos.Users;
using GastroHub.Models;
using GastroHub.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GastroHub.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        public async Task<UserDto> RegisterAsync(RegisterDto dto)
        {
            var user = new AppUser { UserName = dto.Email, Email = dto.Email, DisplayName = dto.DisplayName };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new Exception(string.Join("; ", errors));
            }

            var token = GenerateJwtToken(user);
            return new UserDto { Id = user.Id, Email = user.Email, DisplayName = user.DisplayName, Token = token };
        }

        public async Task<UserDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
                       ?? throw new Exception("Invalid credentials");

            var res = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!res.Succeeded)
                throw new Exception("Invalid credentials");

            var token = GenerateJwtToken(user);
            return new UserDto { Id = user.Id, Email = user.Email, DisplayName = user.DisplayName, Token = token };
        }

        public async Task<UserDto> GetUserAsync(string email)
        {
            var user = await _userManager.Users
                .Include(u => u.LikedRecipes)
                .ThenInclude(lr => lr.Recipe)
                .FirstOrDefaultAsync(u => u.Email == email)
                ?? throw new Exception("User not found");

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Token = GenerateJwtToken(user),
                LikedRecipes = user.LikedRecipes.Select(lr => lr.Recipe.Id).ToList()
            };
        }

        public async Task<UserDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                       ?? throw new Exception("User not found");

            if (!string.IsNullOrWhiteSpace(dto.DisplayName))
                user.DisplayName = dto.DisplayName;
            if (!string.IsNullOrWhiteSpace(dto.Bio))
                user.Bio = dto.Bio;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new Exception(string.Join("; ", errors));
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Token = GenerateJwtToken(user)
            };
        }

        private string GenerateJwtToken(AppUser user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(7);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
