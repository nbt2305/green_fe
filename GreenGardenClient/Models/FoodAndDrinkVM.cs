using System.ComponentModel.DataAnnotations;

namespace GreenGardenClient.Models
{
    public class FoodAndDrinkVM
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public decimal Price { get; set; }
        //public int CategoryId { get; set; }
        public string? Description { get; set; }
        public string CategoryName { get; set; }
        public string? ImgUrl { get; set; }
        public int Quantity { get; set; }
        public bool? Status { get; set; }
    }
    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
    public class FoodAndDrinkVMNew
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public decimal? Price { get; set; }
        //public int CategoryId { get; set; }
        public string? Description { get; set; }
        public string CategoryName { get; set; }
        public string? ImgUrl { get; set; }
        public bool? Status { get; set; }
        public int CategoryId { get; set; }
    }
    public class AddFoodAndDrinkVM
    {
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Giá không được để trống.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0.")]
        public decimal? Price { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Tên món ăn không được để trống.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên món ăn phải có ít nhất 3 ký tự và không quá 100 ký tự.")]
        public string ItemName { get; set; }

        public string? ImgUrl { get; set; }

        [Required(ErrorMessage = "Loại món không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Loại món không được để trống")]
        public int CategoryId { get; set; }
        public bool? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
    public class UpdateFoodAndDrinkVM
    {
        public int ItemId { get; set; }

        public decimal? Price { get; set; }
        public int? QuantityAvailable { get; set; }
        public string? Description { get; set; }
        public string ItemName { get; set; }
        public string? ImgUrl { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
    }
}
