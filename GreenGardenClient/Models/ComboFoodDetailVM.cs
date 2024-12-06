namespace GreenGardenClient.Models
{
    public class ComboFoodDetailVM
    {
        public int ComboId { get; set; }
        public string ComboName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImgUrl { get; set; }
        public int Quantity { get; set; }

        public List<FootComboItemDTO> FootComboItems { get; set; }
    }
    public class FootComboItemDTO
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public int? Quantity { get; set; }
    }
}
