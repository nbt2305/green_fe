using System.ComponentModel.DataAnnotations;

namespace GreenGardenClient.Models
{
    public class UserRegisterVM
    {

        [Required(ErrorMessage = "Họ là bắt buộc.")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Họ không được chứa số.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Tên là bắt buộc.")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Tên không được chứa số.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]

        public string Password { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "Số điện thoại không hợp lệ (chỉ chứa số từ 10 ký tự).")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc.")]
        [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }


    }
}
