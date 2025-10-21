using EVDealerDbContext.Models;
using EVDealerDbContext;
using Microsoft.EntityFrameworkCore;
using Business_Logic_Layer.DTOs;

namespace Business_Logic_Layer.Services
{
    public class DealerService : IDealerService
    {
        private readonly EVDealerSystemContext _context;

        public DealerService(EVDealerSystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DealerDTO>> GetAllDealersAsync()
        {
            var dealers = await _context.Dealers
                .Where(d => d.IsActive == true)
                .OrderBy(d => d.Name)
                .ToListAsync();
                
            return dealers.Select(d => new DealerDTO
            {
                Id = d.Id,
                Name = d.Name,
                Address = d.Address,
                Phone = d.Phone,
                Email = d.Email,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt
            });
        }

        public async Task<DealerDTO?> GetDealerByIdAsync(Guid id)
        {
            var dealer = await _context.Dealers
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive == true);
                
            if (dealer == null) return null;
            
            return new DealerDTO
            {
                Id = dealer.Id,
                Name = dealer.Name,
                Address = dealer.Address,
                Phone = dealer.Phone,
                Email = dealer.Email,
                IsActive = dealer.IsActive,
                CreatedAt = dealer.CreatedAt
            };
        }
    }
}
