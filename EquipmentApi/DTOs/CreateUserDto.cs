namespace EquipmentApi.Dtos
{

    public class CreateUserDto
    {
        public required string EmployeeId { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public required string Password { get; set; }
    }

    public class UpdateUserDto
    {
        public string? EmployeeId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }

        public string? Password { get; set; }
    }
}
