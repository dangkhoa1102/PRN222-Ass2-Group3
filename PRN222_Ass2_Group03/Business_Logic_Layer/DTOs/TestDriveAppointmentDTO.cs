namespace Business_Logic_Layer.DTOs
{
    public class TestDriveAppointmentDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid DealerId { get; set; }
        public Guid VehicleId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties as DTOs
        public UserDTO? Customer { get; set; }
        public DealerDTO? Dealer { get; set; }
        public VehicleDTO? Vehicle { get; set; }
    }
}
