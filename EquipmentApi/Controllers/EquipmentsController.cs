using EquipmentApi.Data;
using EquipmentApi.DTOs;
using EquipmentApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquipmentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EquipmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public EquipmentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var equipments = await _context.Equipments.ToListAsync();

            return Ok(equipments);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment == null) return NotFound("Equipment not found.");

            return Ok(equipment);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateEquipmentDto request)
        {
            // 1. Validation (เหมือนเดิม)
            if (string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.Name))
            {
                return BadRequest("Code and Name are required.");
            }

            if (await _context.Equipments.AnyAsync(e => e.Code == request.Code))
            {
                return BadRequest($"Equipment Code '{request.Code}' already exists.");
            }

            // 2. แปลงไฟล์รูปเป็น Base64 (ส่วนที่แก้) 🛠️
            string imageUrl = "";

            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                // ใช้ MemoryStream เพื่ออ่านไฟล์เป็น byte[] โดยไม่ต้องเซฟลง Disk
                using (var ms = new MemoryStream())
                {
                    await request.ImageFile.CopyToAsync(ms);
                    var fileBytes = ms.ToArray();

                    // แปลงเป็น Base64 String
                    string base64String = Convert.ToBase64String(fileBytes);

                    // จัด Format ให้ Browser อ่านได้เลย (data:image/png;base64,....)
                    imageUrl = $"data:{request.ImageFile.ContentType};base64,{base64String}";
                }
            }

            // 3. สร้าง Object ลง DB
            var newEquipment = new Equipment
            {
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                CategoryId = request.CategoryId,
                ImageUrl = imageUrl, // เก็บสตริงยาวๆ ลง DB ไปเลย
                Stock = request.Stock, // อย่าลืม mapping field อื่นๆ ให้ครบ
                IsUnlimited = request.IsUnlimited,
                Status = 1
            };

            _context.Equipments.Add(newEquipment);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return Ok(new
                {
                    message = "Equipment created successfully!",
                    data = newEquipment,
                    timestamp = DateTime.Now
                });
            }
            else
            {
                return StatusCode(500, "Database save failed.");
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put(Guid id, CreateEquipmentDto request)
        {
            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment == null) return NotFound("Equipment not found.");

            equipment.Code = request.Code;
            equipment.Name = request.Name;
            equipment.Description = request.Description;
            equipment.CategoryId = request.CategoryId;

            if (request.ImageFile != null)
            {

                if (!string.IsNullOrEmpty(equipment.ImageUrl))
                {

                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", equipment.ImageUrl.TrimStart('/'));


                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ImageFile.FileName);
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                string filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImageFile.CopyToAsync(stream);
                }

                equipment.ImageUrl = $"/images/{fileName}";
            }


            await _context.SaveChangesAsync();

            return Ok(equipment);

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment == null) return BadRequest();


            _context.Equipments.Remove(equipment);
            await _context.SaveChangesAsync();
            return Ok("Equipment deleted.");
        }


    }
}
