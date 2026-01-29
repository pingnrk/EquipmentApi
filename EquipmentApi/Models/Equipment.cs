namespace EquipmentApi.Models
{
    public class Equipment
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public int Status { get; set; }
        public int Stock { get; set; } = 1;
        public bool IsUnlimited { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
