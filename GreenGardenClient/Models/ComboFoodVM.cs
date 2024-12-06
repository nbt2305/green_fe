namespace GreenGardenClient.Models
{
    public class ComboFoodVM
    {
        public int ComboId { get; set; }
        public string ComboName { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImgUrl { get; set; }
        public int Quantity { get; set; }

    }
}
