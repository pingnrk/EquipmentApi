using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EquipmentApi.Data;
using EquipmentApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EquipmentApi.Models;

namespace EquipmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
            .OrderByDescending(u => u.IsActive)
            .ThenBy(u => u.FullName)
            .ToListAsync();

            return Ok(users);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser(CreateUserDto request)
        {
            if (await _context.Users.AnyAsync(u => u.EmployeeId == request.EmployeeId.Trim().ToLower()))
            {
                return BadRequest(new { message = "รหัสพนักงานนี้ถูกใช้งานแล้ว" });
            }

            var newUser = new User
            {
                EmployeeId = request.EmployeeId,
                FullName = request.FullName,
                Email = request.Email,
                Role = request.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true // เริ่มต้นเป็น True เสมอ
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok(newUser);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "ไม่พบผู้ใช้งานนี้" });

            if (request.EmployeeId != null) user.EmployeeId = request.EmployeeId;
            if (request.FullName != null) user.FullName = request.FullName;
            if (request.Email != null) user.Email = request.Email;
            if (request.Role != null) user.Role = request.Role;

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return BadRequest(new { message = "ไม่พบผู้ใช้งานนี้" });

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "แก้ไขข้อมูลสำเร็จ"});
        }






    }
}
