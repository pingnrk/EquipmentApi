namespace EquipmentApi.Models
{
    public class BorrowRequestItem
    {
        public Guid Id { get; set; }
        public Guid BorrowRequestId { get; set; }
        public Guid EquipmentId { get; set; }
        public int ItemStatus { get; set; } = 1;
        public int Quantity { get; set; } = 1;
    }
}
