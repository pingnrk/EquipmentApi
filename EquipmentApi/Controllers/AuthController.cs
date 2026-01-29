using EquipmentApi.Data;
using EquipmentApi.DTOs;
using EquipmentApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EquipmentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController] // ตัวนี้จะ Auto-Validate ตาม Data Annotations ใน DTO ให้เอง ถ้าไม่ผ่านมันจะ Return 400 ทันที
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            var cleanEmpId = request.EmployeeId.Trim().ToLower();
            var cleanEmail = request.Email.Trim().ToLower();

            if (await _context.Users.AnyAsync(u => u.EmployeeId == cleanEmpId))
            {
                return BadRequest(new { message = "รหัสพนักงานนี้ถูกใช้งานแล้ว" });
            }

            if (await _context.Users.AnyAsync(u => u.Email == cleanEmail))
            {
                return BadRequest(new { message = "อีเมลนี้ถูกใช้งานแล้ว" });
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                EmployeeId = cleanEmpId,
                FullName = request.FullName.Trim(),
                Email = cleanEmail,
                PasswordHash = passwordHash,
                Role = "Member",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "ลงทะเบียนสำเร็จ!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var cleanEmpId = request.EmployeeId.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == cleanEmpId);

            if (user == null)
            {
                return Unauthorized(new { message = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง" });
            }
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง" });
            }

            string token = CreateToken(user);

            return Ok(new
            {
                Token = token,
                Role = user.Role,
                FullName = user.FullName
            });
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("JwtSettings:Key").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
