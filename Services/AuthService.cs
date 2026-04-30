using MCLS.Data;
using MCLS.Dto;
using MCLS.Generics;
using MCLS.IServices;
using MCLS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MCLS.Services
{
    public class AuthService(UserManager<User> _userManager, RoleManager<IdentityRole> _roleManager,
        IConfiguration _configuration, AppDbContext _context) : IAuthService
    {
        public async Task<ServiceResponse<List<string>>> Register(RegisterDto data)
        {
            try
            {
                if (data.Name == null || data.Email == null || data.Password == null || data.Rank == null)
                {
                    return ServiceResponse<List<string>>.Failure("Name, Email, Password, Rank are required", null, 400);
                }
                if (!await _roleManager.RoleExistsAsync(data.Rank))
                {
                    return ServiceResponse<List<string>>.Failure("Rank does not exist, please create this rank first", null, 400);
                }

                var userExists = await _userManager.FindByEmailAsync(data.Email);
                if (userExists != null)
                {
                    return ServiceResponse<List<string>>.Failure("User already exists", null, 400);
                }
                var user = new User { UserName = data.Email, Name = data.Name, Email = data.Email, Rank = data.Rank };

                if (!string.IsNullOrWhiteSpace(data.VesselId))
                {
                    Guid vesselId;
                    if (!Guid.TryParse(data.VesselId, out vesselId))
                    {
                        return ServiceResponse<List<string>>.Failure("Invalid vesselId", null, 400);
                    }

                    var vessel = await _context.Vessels.AnyAsync(v => v.Id == vesselId);
                    if (!vessel)
                    {
                        return ServiceResponse<List<string>>.Failure("Vessel not found", null, 404);
                    }
                    user = new User { UserName = data.Email, Name = data.Name, Email = data.Email, Rank = data.Rank, VesselId = vesselId };

                }

                var register = await _userManager.CreateAsync(user, data.Password);
                if (!register.Succeeded)
                {
                    return ServiceResponse<List<string>>.Failure("Validation failed", register.Errors.Select(x => x.Description).ToList(), 400);
                }
                await _userManager.AddToRoleAsync(user, user.Rank);

                return ServiceResponse<List<string>>.Success("User created successfully", null, 201);
            }
            catch (Exception)
            {
                return ServiceResponse<List<string>>.Failure("Internal server error", null, 500);
                throw;
            }
        }

        public async Task<ServiceResponse<string>> Login(RegisterDto data)
        {
            try
            {
                if (data.Email == null || data.Password == null)
                {
                    return ServiceResponse<string>.Failure("Email and Password are required", null, 400);
                }
                var existingUser = await _userManager.FindByEmailAsync(data.Email);
                if (existingUser == null)
                {
                    return ServiceResponse<string>.Failure("User not found", null, 404);
                }
                var verifyPass = await _userManager.CheckPasswordAsync(existingUser, data.Password);
                if (!verifyPass)
                {
                    return ServiceResponse<string>.Failure("Invalid password", null, 400);
                }
                string token = GetJwtToken(existingUser, await _userManager.GetRolesAsync(existingUser));
                return ServiceResponse<string>.Success("Login successfully", token, 200);

            }
            catch (Exception)
            {
                return ServiceResponse<string>.Failure("Internal server error", null, 500);
                throw;
            }
        }

        private string GetJwtToken(User user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("VesselId", user.VesselId?.ToString() ?? "")
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    signingCredentials: creds,
                    expires: DateTime.UtcNow.AddHours(24)
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ServiceResponse<List<string>>> UpdateUser(RegisterDto userDetails, string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResponse<List<string>>.Failure("Invalid userId", null, 400);
                }
                if (!string.IsNullOrWhiteSpace(userDetails.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, userDetails.Password);
                    if (!result.Succeeded)
                    {
                        return ServiceResponse<List<string>>.Failure("Invalid password", result.Errors.Select(e => e.Description).ToList(), 400);
                    }
                }

                user.Name = userDetails.Name ?? user.Name;
                user.Email = userDetails.Email ?? user.Email;
                user.Rank = userDetails.Rank ?? user.Rank;
                if (!string.IsNullOrWhiteSpace(userDetails.VesselId))
                {
                    Guid vesselId;
                    if (!Guid.TryParse(userDetails.VesselId, out vesselId))
                    {
                        return ServiceResponse<List<string>>.Failure("Invalid vesselId", null, 400);
                    }
                    var vessel = await _context.Vessels.AnyAsync(v => v.Id == vesselId);
                    if (!vessel)
                    {
                        return ServiceResponse<List<string>>.Failure("Vessel not found", null, 404);
                    }
                    user.VesselId = vesselId;
                }
                await _userManager.UpdateAsync(user);
                return ServiceResponse<List<string>>.Success("Updated successfully!", null, 200);
            }
            catch (Exception)
            {
                return ServiceResponse<List<string>>.Failure("Internal server error", null, 500);
                throw;
            }

        }

    }
}
