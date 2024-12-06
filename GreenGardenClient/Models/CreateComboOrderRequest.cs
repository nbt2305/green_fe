namespace GreenGardenClient.Models
{
    public class CreateComboOrderRequest
    {
        public OrderAddDTO Order { get; set; }
        public List<OrderComboAddDTO> OrderCombo { get; set; }
        public List<OrderCampingGearAddDTO> OrderCampingGear { get; set; }
        public List<OrderFoodAddDTO> OrderFood { get; set; }
        public List<OrderFoodComboAddDTO> OrderFoodCombo { get; set; }
    }
    public class OrderComboAddDTO
    {
        public int ComboId { get; set; }
        public int OrderId { get; set; }
        public int? Quantity { get; set; }
        public string? Description { get; set; }
    }

}
    

