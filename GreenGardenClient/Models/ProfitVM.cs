namespace GreenGardenClient.Models
{
    public class ProfitVM
    {
        public decimal TotalAmount { get; set; }
        public int TotalOrderOnline { get; set; }
        public int TotalDepositOrderOnline { get; set; }
        public decimal MoneyTotalDepositOrderOnline { get; set; }
        public int TotalOrderCancel { get; set; }
        public int TotalDepositOrderCancel { get; set; }
        public decimal MoneyTotalDepositOrderCancel { get; set; }
        public int TotalOrderUsing { get; set; }
        public int TotalDepositOrderUsing { get; set; }
        public decimal MoneyTotalDepositOrderUsing { get; set; }
        public int TotalOrderCheckout { get; set; }
        public decimal MoneyTotalAmountOrderCheckout { get; set; }
    }
}
