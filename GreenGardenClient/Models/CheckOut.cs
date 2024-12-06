namespace GreenGardenClient.Models
{
    public class CheckOut
    {
        public CustomerOrderAddDTO Order { get; set; }
        public List<CustomerOrderTicketAddlDTO> OrderTicket { get; set; }
        public List<CustomerOrderCampingGearAddDTO> OrderCampingGear { get; set; }
        public List<CustomerOrderFoodAddDTO> OrderFood { get; set; }
        public List<CustomerOrderFoodComboAddDTO> OrderFoodCombo { get; set; }
    }
    public class CheckOutComboOrderRequest
    {
        public CustomerOrderAddDTO Order { get; set; }
        public List<CustomerOrderComboAddDTO> OrderCombo { get; set; }
        public List<CustomerOrderCampingGearAddDTO> OrderCampingGear { get; set; }
        public List<CustomerOrderFoodAddDTO> OrderFood { get; set; }
        public List<CustomerOrderFoodComboAddDTO> OrderFoodCombo { get; set; }
    }
    public class CustomerOrderComboAddDTO
    {
        public int ComboId { get; set; }
        public int OrderId { get; set; }
        public int? Quantity { get; set; }
        public string? Description { get; set; }
        public string? ImgUrl { get; set; }
    }
    public class CustomerOrderAddDTO
    {
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? OrderUsageDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal Deposit { get; set; }
        public decimal TotalAmount { get; set; }

        public string? PhoneCustomer { get; set; }

    }
    public class CustomerOrderTicketAddlDTO
    {
        public int TicketId { get; set; }
        public int OrderId { get; set; }
        public int? Quantity { get; set; }
        public string? ImgUrl { get; set; }
    }
    public class CustomerOrderCampingGearAddDTO
    {
        public int GearId { get; set; }
        public int OrderId { get; set; }

        public string? ImgUrl { get; set; }

        public int? Quantity { get; set; }
    }
    public class CustomerOrderFoodAddDTO
    {
        public int ItemId { get; set; }
        public int OrderId { get; set; }

        public string? ImgUrl { get; set; }
        public int? Quantity { get; set; }
        public string? Description { get; set; }
    }
    public class CustomerOrderFoodComboAddDTO
    {
        public int ComboId { get; set; }
        public int OrderId { get; set; }
        public int? Quantity { get; set; }
        public string? ImgUrl { get; set; }

    }
}
