using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquipmentApi.Models
{
    public class BorrowRequest
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public int Status { get; set; }

        public Guid? ApprovedBy { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<BorrowRequestItem> Items { get; set; } = new List<BorrowRequestItem>();
    }
}