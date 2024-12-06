using System.ComponentModel.DataAnnotations;

public class Account
{
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? RoleId { get; set; } // Nullable int
}
public class UpdateProfile
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Họ không được để trống.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Tên không được để trống.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
    public string Email { get; set; } = null!;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải có đúng 10 ký tự.")]
    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
public class ChangePassword
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    public string OldPassword { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [StringLength(20, ErrorMessage = "Mật khẩu mới phải từ {2} đến {1} ký tự.", MinimumLength = 6)] // Đảm bảo mật khẩu có ít nhất 6 ký tự.
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
    [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu mới không khớp.")]
    public string ConfirmPassword { get; set; } = null!;
}
public class Employee
{
    [Required(ErrorMessage = "Họ không được để trống.")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Tên không được để trống.")]

    public string LastName { get; set; }

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]

    public string Password { get; set; }
    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải có đúng 10 ký tự.")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]

    public string Address { get; set; }
    [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]

    public DateTime? DateOfBirth { get; set; }
    [Required(ErrorMessage = "Giới tính là bắt buộc.")]

    public string Gender { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}