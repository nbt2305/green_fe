using System.ComponentModel.DataAnnotations;

namespace GreenGardenClient.Models
{
    public class GearVM
    {
        public int GearId { get; set; }
        public string GearName { get; set; } = null!;
        public int QuantityAvailable { get; set; }
        public decimal RentalPrice { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string GearCategoryName { get; set; }
        public string? ImgUrl { get; set; }
        public int Quantity { get; set; }
        public bool? Status { get; set; }
    }
    public class GearDetailVM
    {
        public int GearId { get; set; }
        public string GearName { get; set; } = null!;
        public int QuantityAvailable { get; set; }
        public decimal RentalPrice { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int GearCategoryId { get; set; }
        public string GearCategoryName { get; set; }
        public string? ImgUrl { get; set; }
        public bool? Status { get; set; }
    }
    public class AddGearVM
    {
        public int GearId { get; set; }
        [Required(ErrorMessage = "Tên thiết bị là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Tên thiết bị không được vượt quá 100 ký tự.")]
        public string GearName { get; set; } = null!;

        [Required(ErrorMessage = "Số lượng thiết bị có sẵn là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng thiết bị phải lớn hơn hoặc bằng 0.")]
        public int? QuantityAvailable { get; set; }

        [Required(ErrorMessage = "Giá thuê là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn hoặc bằng 0.")]
        public decimal? RentalPrice { get; set; }

        [MaxLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        public DateTime? CreatedAt { get; set; }

        [Required(ErrorMessage = "Danh mục thiết bị là bắt buộc.")]
        public int? GearCategoryId { get; set; }
        public string? ImgUrl { get; set; }

        public bool? Status { get; set; }

    }
    public class UpdateGearVM
    {
        public int GearId { get; set; }
        [Required(ErrorMessage = "Tên thiết bị là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Tên thiết bị không được vượt quá 100 ký tự.")]
        public string GearName { get; set; } = null!;
        [Required(ErrorMessage = "Số lượng thiết bị có sẵn là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng thiết bị phải lớn hơn 0.")]
        public int? QuantityAvailable { get; set; }
        [Required(ErrorMessage = "Giá thuê là bắt buộc.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn 0.")]
        public decimal? RentalPrice { get; set; }
        public string? Description { get; set; }
        public int? GearCategoryId { get; set; }
        public string? ImgUrl { get; set; }
    }
    public class OrderCampingGearByUsageDateDTO
    {
        public int GearId { get; set; }
        public int? Quantity { get; set; }
    }

}
