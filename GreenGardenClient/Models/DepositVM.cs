using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace GreenGardenClient.Models
{
    public class DepositVM
    {
        public decimal Total { get; set; }
        public decimal Rounded20 { get; set; }
        public decimal Rounded40 { get; set; }
        public decimal Rounded60 { get; set; }
        public decimal Rounded80 { get; set; }
        public decimal Rounded100 { get; set; }

        public void CalculateRoundedValues()
        {
            // Gọi phương thức làm tròn cho các tỷ lệ khác nhau của Total
            Rounded20 = RoundToNearest(Total * 0.2m);
            Rounded40 = RoundToNearest(Total * 0.4m);
            Rounded60 = RoundToNearest(Total * 0.6m);
            Rounded80 = RoundToNearest(Total * 0.8m);
            Rounded100 = RoundToNearest(Total * 1m);
        }

        private decimal RoundToNearest(decimal amount)
        {
            // Kiểm tra giá trị và làm tròn theo quy tắc
            if (amount < 10)
            {
                return 0; // Nếu số tiền nhỏ hơn 10, trả về 0 hoặc có thể bỏ qua
            }
            else if (amount < 100) // Hàng chục
            {
                return Math.Round(amount / 10) * 10;
            }
            else if (amount < 1_000) // Hàng trăm
            {
                return Math.Round(amount / 100) * 100;
            }
            else if (amount < 1_000_000) // Hàng triệu
            {
                return Math.Round(amount / 1_000) * 1_000;
            }
            else if (amount < 10_000_000) // Hàng chục triệu
            {
                return Math.Round(amount / 10_000_000) * 10_000_000;
            }
            else // Hàng triệu trở lên
            {
                return Math.Round(amount / 1_000_000) * 1_000_000;
            }
        }
    }

}

