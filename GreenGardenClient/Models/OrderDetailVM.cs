namespace GreenGardenClient.Models
{
    public class OrderDetailVM
    {
        public int OrderId { get; set; }
        public string? EmployeeName { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? OrderUsageDate { get; set; }
        public decimal Deposit { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPayable { get; set; }
        public bool? StatusOrder { get; set; }
        public int? ActivityId { get; set; }
        public string? ActivityName { get; set; }
        public string? PhoneCustomer { get; set; }
        public DateTime? OrderCheckoutDate { get; set; }

        public virtual ICollection<OrderCampingGearDetailDTO> OrderCampingGearDetails { get; set; }
        public virtual ICollection<OrderComboDetailDTO> OrderComboDetails { get; set; }
        public virtual ICollection<OrderFoodComboDetailDTO> OrderFoodComboDetails { get; set; }
        public virtual ICollection<OrderFoodDetailDTO> OrderFoodDetails { get; set; }
        public virtual ICollection<OrderTicketDetailDTO> OrderTicketDetails { get; set; }
    }
    public class OrderCampingGearDetailDTO
    {
        public int GearId { get; set; }
        public int OrderId { get; set; }

        public string Name { get; set; }
        public int? QuantityAvaiable { get; set; }
        public string? ImgUrl { get; set; }
        public int? Quantity { get; set; }
        public decimal Price { get; set; }

        public string? Description { get; set; }
    }
    public class OrderFoodComboDetailDTO
    {
        public int ComboId { get; set; }
        public int OrderId { get; set; }

        public string Name { get; set; }
        public string? ImgUrl { get; set; }
        public int? Quantity { get; set; }
        public decimal Price { get; set; }

        public string? Description { get; set; }

    }
    public class OrderFoodDetailDTO
    {
        public int ItemId { get; set; }
        public int OrderId { get; set; }
        public string? ImgUrl { get; set; }
        public string Name { get; set; }

        public int? Quantity { get; set; }
        public decimal Price { get; set; }

        public string? Description { get; set; }

    }
    public class OrderComboDetailDTO
    {
        public int ComboId { get; set; }
        public string Name { get; set; }

        public int? Quantity { get; set; }
        public decimal Price { get; set; }
        public string? ImgUrl { get; set; }
        public string? Description { get; set; }
    }
    public class OrderTicketDetailDTO
    {
        public int TicketId { get; set; }
        public string? ImgUrl { get; set; }
        public string Name { get; set; }

        public int? Quantity { get; set; }

        public decimal Price { get; set; }

        public string? Description { get; set; }
    }
}
