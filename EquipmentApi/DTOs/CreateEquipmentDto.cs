namespace EquipmentApi.DTOs
{
    public class CreateEquipmentDto
    {
        public required string Code { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public IFormFile? ImageFile { get; set; }

        public int Stock { get; set; } = 1;
        public bool IsUnlimited { get; set; } = false;
    }
}
