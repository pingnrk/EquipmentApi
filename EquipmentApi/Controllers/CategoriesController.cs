using EquipmentApi.Data;
using EquipmentApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquipmentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var result = await _context.Categories.ToListAsync();

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Category(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (category == null) return BadRequest();

            if (await _context.Categories.AnyAsync(c => c.Name == category.Name))
            {
                return BadRequest("ชื่อหมวดหมู่นี้มีอยู่แล้ว");
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Category), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if (id != category.Id) return BadRequest("ID ไม่ถูกต้อง");

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null) return NotFound();

            existingCategory.Name = category.Name;

            await _context.SaveChangesAsync();
            return Ok(new { message = "แก้ไขเรียบร้อย" });

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "ลบหมวดหมู่เรียบร้อย" });
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }




    }
}
