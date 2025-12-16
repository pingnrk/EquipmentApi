using System.ComponentModel.DataAnnotations;

namespace EquipmentApi.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "กรุณากรอกรหัสพนักงาน")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "รหัสพนักงานต้องมีความยาว 3-20 ตัวอักษร")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "รหัสพนักงานต้องเป็นภาษาอังกฤษหรือตัวเลขเท่านั้น")]
        public string EmployeeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกชื่อ-นามสกุล")]
        [StringLength(100, ErrorMessage = "ชื่อยาวเกินไป")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง (ต้องมี @ และลงท้ายด้วย .com หรือ .net เป็นต้น)")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [MinLength(6, ErrorMessage = "รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร")]
        public string Password { get; set; } = string.Empty;
    }
}
