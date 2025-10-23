namespace Business_Logic_Layer.DTOs
{
    public class VehicleDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? Specifications { get; set; }
        public string? Images { get; set; }
        public int StockQuantity { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
