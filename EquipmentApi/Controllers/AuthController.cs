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
    [ApiController]
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

            if (await _context.Users.AnyAsync(u => u.EmployeeId == cleanEmpId))
            {
                return BadRequest("Employee ID already exists.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                EmployeeId = cleanEmpId,
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = "Member"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            // 1. จัด Format Input ให้เป็นตัวเล็กเหมือนใน DB
            var cleanEmpId = request.EmployeeId.Trim().ToLower();

            // 2. หา User จาก EmployeeId แทน Email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == cleanEmpId);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // 3. เช็ค Password (เหมือนเดิม)
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Wrong password.");
            }

            // 4. สร้าง Token (เหมือนเดิม)
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
            new Claim(ClaimTypes.Role, user.Role) // ใส่ Role เข้าไปใน Token เลย
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("JwtSettings:Key").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1), // Token หมดอายุใน 1 วัน
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
