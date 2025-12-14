namespace EquipmentApi.Models
{
    public class BorrowRequest
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Status { get; set; }


        public Guid? ApprovedBy { get; set; }

        public DateTime? ReturnDate { get; set; }

    }
}
