namespace GreenGardenClient.Models
{
    public class TicketCategoryVM

    {
        public int TicketCategoryId { get; set; }
        public string TicketCategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
