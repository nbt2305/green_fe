namespace GreenGardenClient.Models
{
    public class OrderVM
    {
        public int OrderId { get; set; }
        public int? CustomerId { get; set; }
        public int? EmployeeId { get; set; }
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

    }
    public class CustomerOrderVM
    {
        public int OrderId { get; set; }
        public int? CustomerId { get; set; }
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

    }
    public class UpdateOrderDTO
    {

        public int OrderId { get; set; }
        public DateTime? OrderUsageDate { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? OrderCheckoutDate { get; set; }

    }
}
