namespace GreenGardenClient.Models
{
    public class GearCategoryVM
    {
        public int GearCategoryId { get; set; }
        public string GearCategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
