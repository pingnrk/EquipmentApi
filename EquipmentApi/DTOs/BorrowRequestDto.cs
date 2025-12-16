namespace EquipmentApi.DTOs
{
    public class BorrowRequestDto
    {

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<BorrowRequestItemDto> Items { get; set; } = new();
    }

    public class BorrowRequestItemDto
    {

        public Guid EquipmentId { get; set; }

        public int Quantity { get; set; }
    }
}