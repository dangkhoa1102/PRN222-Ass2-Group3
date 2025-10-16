using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer.DTOs
{
    public class ConfirmOrderDTO
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid StaffId { get; set; }

        public string Notes { get; set; }
    }
}
