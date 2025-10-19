using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Business_Logic_Layer.DTOs
{
    public class CreateOrderDTO
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid DealerId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        public string Notes { get; set; } = string.Empty;
    }
}
