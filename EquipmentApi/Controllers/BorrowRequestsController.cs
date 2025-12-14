using EquipmentApi.Data;
using EquipmentApi.DTOs;
using EquipmentApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EquipmentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BorrowRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BorrowRequestsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest(BorrowRequestDto request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = Guid.Parse(userIdString);

            if (request.Items.Count == 0) return BadRequest("No items selected.");
            if (request.EndDate < request.StartDate) return BadRequest("End date must be after start date.");

            var requestEquipmentIds = request.Items.Select(i => i.EquipmentId).ToList();

            var equipmentsInDb = await _context.Equipments
                .Where(e => requestEquipmentIds.Contains(e.Id))
                .ToListAsync();


            foreach (var itemRequest in request.Items)
            {
                var equipment = equipmentsInDb.FirstOrDefault(e => e.Id == itemRequest.EquipmentId);


                if (equipment == null)
                    return BadRequest($"Equipment ID {itemRequest.EquipmentId} not found.");


                if (equipment.Stock < itemRequest.Quantity)
                {
                    return BadRequest($"Not enough stock for '{equipment.Name}'. Available: {equipment.Stock}, Requested: {itemRequest.Quantity}");
                }
            }


            var borrowRequest = new BorrowRequest
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = 1,
                RequestDate = DateTime.UtcNow
            };
            _context.BorrowRequests.Add(borrowRequest);
            await _context.SaveChangesAsync();


            var requestItems = new List<BorrowRequestItem>();

            foreach (var itemRequest in request.Items)
            {

                requestItems.Add(new BorrowRequestItem
                {
                    BorrowRequestId = borrowRequest.Id,
                    EquipmentId = itemRequest.EquipmentId,
                    Quantity = itemRequest.Quantity,
                    ItemStatus = 1
                });


                var equipment = equipmentsInDb.First(e => e.Id == itemRequest.EquipmentId);
                equipment.Stock -= itemRequest.Quantity;

                if (equipment.Stock == 0) equipment.Status = 2;
            }

            await _context.Set<BorrowRequestItem>().AddRangeAsync(requestItems);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request submitted", requestId = borrowRequest.Id });
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var requests = await _context.BorrowRequests
                .Where(r => r.Status == 1)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new
                {
                    r.Id,
                    r.RequestDate,
                    r.EndDate,
                    UserName = _context.Users.Where(u => u.Id == r.UserId).Select(u => u.FullName).FirstOrDefault(),
                    ItemCount = _context.BorrowRequestItems.Count(i => i.BorrowRequestId == r.Id)

                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRequest(Guid id)
        {
            var request = await _context.BorrowRequests.FindAsync(id);
            if (request == null) return NotFound("Request not found.");

            if (request.Status != 1) return BadRequest("Request is not in Pending status.");

            request.Status = 2;

            var adminIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(adminIdString))
            {
                request.ApprovedBy = Guid.Parse(adminIdString);
            }

            var itemIds = await _context.BorrowRequestItems
                .Where(x => x.BorrowRequestId == id)
                .Select(x => x.EquipmentId)
                .ToListAsync();

            var equipments = await _context.Equipments
                .Where(e => itemIds.Contains(e.Id))
                .ToListAsync();

            foreach (var eq in equipments)
            {
                eq.Status = 2;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Request approved", requestId = id });
        }

        [HttpPut("{id}/return")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReturnRequest(Guid id)
        {
            var request = await _context.BorrowRequests.FindAsync(id);
            if (request == null) return NotFound();
            if (request.Status != 2) return BadRequest("Invalid status.");

            // Update Request Status
            request.Status = 4; // Returned
            request.ReturnDate = DateTime.UtcNow;

            // ดึงรายการที่เคยยืมไป
            var borrowedItems = await _context.BorrowRequestItems
                .Where(x => x.BorrowRequestId == id)
                .ToListAsync();

            // ดึงข้อมูลของในคลังมาอัปเดต
            var equipmentIds = borrowedItems.Select(x => x.EquipmentId).ToList();
            var equipments = await _context.Equipments
                .Where(e => equipmentIds.Contains(e.Id))
                .ToListAsync();

            foreach (var borrowedItem in borrowedItems)
            {
                var equipment = equipments.FirstOrDefault(e => e.Id == borrowedItem.EquipmentId);
                if (equipment != null)
                {
                    // **คืน Stock กลับเข้าไป**
                    equipment.Stock += borrowedItem.Quantity;

                    // ถ้าของกลับมามี Stock ให้เปลี่ยนสถานะกลับเป็น Available (1)
                    if (equipment.Stock > 0) equipment.Status = 1;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Items returned and stock updated." });
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectRequest(Guid id)
        {
            var request = await _context.BorrowRequests.FindAsync(id);
            if (request == null) return NotFound("Request not found.");

            if (request.Status != 1) return BadRequest("Can only reject Pending requests.");

            // 1. เปลี่ยนสถานะใบคำขอเป็น Rejected (3)
            request.Status = 3;

            // บันทึกคนกดปฏิเสธ
            var adminIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(adminIdString))
            {
                request.ApprovedBy = Guid.Parse(adminIdString);
            }

            // -----------------------------------------------------------
            // 2. (เพิ่มส่วนนี้) คืน Stock กลับสู่คลัง
            // -----------------------------------------------------------

            // ดึงรายการที่ User เคยขอจองไว้
            var requestItems = await _context.BorrowRequestItems
                .Where(ri => ri.BorrowRequestId == id)
                .ToListAsync();

            // ดึงของในคลังออกมา
            var equipmentIds = requestItems.Select(ri => ri.EquipmentId).ToList();
            var equipmentsInDb = await _context.Equipments
                .Where(e => equipmentIds.Contains(e.Id))
                .ToListAsync();

            foreach (var reqItem in requestItems)
            {
                var equipment = equipmentsInDb.FirstOrDefault(e => e.Id == reqItem.EquipmentId);
                if (equipment != null)
                {
                    // คืน Stock ตามจำนวนที่จองไว้
                    equipment.Stock += reqItem.Quantity;

                    // ถ้าของกลับมามี Stock ให้เปลี่ยนสถานะกลับเป็น Available (1)
                    // (เผื่อก่อนหน้านี้มันเหลือ 0 จนกลายเป็น InUse ไป)
                    if (equipment.Stock > 0) equipment.Status = 1;
                }
            }
            // -----------------------------------------------------------

            await _context.SaveChangesAsync();

            return Ok(new { message = "Request rejected and stock returned.", requestId = id });
        }
    }
}
