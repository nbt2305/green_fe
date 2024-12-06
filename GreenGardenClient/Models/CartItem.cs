namespace GreenGardenClient.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public DateTime UsageDate { get; set; }
        public string? Image { get; set; }
        public string Type { get; set; }
        public string TypeCategory { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
        public int? QuantityAvailable { get; set; }
    }
}
