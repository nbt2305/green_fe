namespace GreenGardenClient.Models
{
    public class CreateUniqueOrderRequest
    {
        public OrderAddDTO Order { get; set; }
        public List<OrderTicketAddlDTO> OrderTicket { get; set; }
        public List<OrderCampingGearAddDTO> OrderCampingGear { get; set; }
        public List<OrderFoodAddDTO> OrderFood { get; set; }
        public List<OrderFoodComboAddDTO> OrderFoodCombo { get; set; }
    }
    public class OrderAddDTO
    {
        public int? EmployeeId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? OrderUsageDate { get; set; }
        public decimal Deposit { get; set; }
        public decimal TotalAmount { get; set; }

        public string? PhoneCustomer { get; set; }

    }
    public class OrderTicketAddlDTO
    {
        public int TicketId { get; set; }
        public int OrderId { get; set; }
        public int? Quantity { get; set; }
    }
    public class OrderCampingGearAddDTO
    {
        public int GearId { get; set; }
        public int OrderId { get; set; }



        public int? Quantity { get; set; }
    }
    public class OrderFoodAddDTO
    {
        public int ItemId { get; set; }
        public int OrderId { get; set; }


        public int? Quantity { get; set; }
        public string? Description { get; set; }
    }
    public class OrderFoodComboAddDTO
    {
        public int ComboId { get; set; }
        public int OrderId { get; set; }
        public int? Quantity { get; set; }

    }
}
