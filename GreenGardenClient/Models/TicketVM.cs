using System.ComponentModel.DataAnnotations;

namespace GreenGardenClient.Models
{
    public class TicketVM
    {
        public int TicketId { get; set; }
        public string TicketName { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImgUrl { get; set; }
        public int Quantity { get; set; }
        public int TicketCategoryId { get; set; }
        public string TicketCategoryName { get; set; }
        public bool Status { get; set; }
    }
    public class TicketDetailVM
    {
        public int TicketId { get; set; }
        public string TicketName { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImgUrl { get; set; }
        public int TicketCategoryId { get; set; }
        public string TicketCategoryName { get; set; }
        public bool Status { get; set; }
    }
    public class UpdateTicketVM
    {
        public int TicketId { get; set; }
        public string TicketName { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImgUrl { get; set; }
        public int TicketCategoryId { get; set; }
    }
    public class AddTicketVM
    {
        public int TicketId { get; set; }
        [Required(ErrorMessage = "Tên vé không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên vé không được vượt quá 100 ký tự.")]
        public string TicketName { get; set; } = null!;
        [Required(ErrorMessage = "Giá vé không được để trống.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá vé phải lớn hơn hoặc bằng 0.")]
        public decimal? Price { get; set; }

        public DateTime CreatedAt { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID danh mục vé không hợp lệ.")]
        public int TicketCategoryId { get; set; }

        public string ImgUrl { get; set; } = null!;
        public bool Status { get; set; }

    }
    public class UpdateTicketDTO
    {
        public int TicketId { get; set; }
        public int OrderId { get; set; }
        public int? Quantity { get; set; }
    }
}
