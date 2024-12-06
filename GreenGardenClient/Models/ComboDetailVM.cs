namespace GreenGardenClient.Models
{
    public class ComboDetailVM
    {
        public int ComboId { get; set; }
        public string ComboName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImgUrl { get; set; }
        public int Quantity { get; set; }

        public virtual List<ComboCampingGearDetailDTO> ComboCampingGearDetails { get; set; }
        public virtual List<ComboFootDetailDTO> ComboFootDetails { get; set; }
        public virtual List<ComboTicketDetailDTO> ComboTicketDetails { get; set; }

    }
    public class ComboFootDetailDTO
    {
        public int ItemId { get; set; }
        public string? Name { get; set; }

        public int? Quantity { get; set; }
        public string? Description { get; set; }
    }
    public class ComboTicketDetailDTO
    {
        public int TicketId { get; set; }
        public string? Name { get; set; }

        public int? Quantity { get; set; }
        public string? Description { get; set; }

    }
    public class ComboCampingGearDetailDTO
    {

        public int ComboId { get; set; }
        public int GearId { get; set; }
        public string Name { get; set; }

        public int? Quantity { get; set; }

    }
}
