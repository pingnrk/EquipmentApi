namespace EquipmentApi.DTOs
{
    public class BorrowRequestItemDto
    {
        public Guid EquipmentId { get; set; }
        public int Quantity { get; set; }
    }

    public class BorrowRequestDto
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        // เปลี่ยนจาก List<Guid> เป็น List<Object> ที่มีจำนวน
        public List<BorrowRequestItemDto> Items { get; set; } = new();
    }
}
