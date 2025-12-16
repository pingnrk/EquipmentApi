using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquipmentApi.Models
{
    public class BorrowRequestItem
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BorrowRequestId { get; set; }

        public Guid EquipmentId { get; set; }

        public int Quantity { get; set; }
        public int ItemStatus { get; set; }

        [ForeignKey("BorrowRequestId")]
        public virtual BorrowRequest? BorrowRequest { get; set; }

        [ForeignKey("EquipmentId")]
        public virtual Equipment? Equipment { get; set; }
    }
}