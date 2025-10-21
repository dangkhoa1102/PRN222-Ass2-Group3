using EVDealerDbContext.Models;
using EVDealerDbContext;
using Microsoft.EntityFrameworkCore;

namespace Business_Logic_Layer.Services
{
    public class DealerService : IDealerService
    {
        private readonly EVDealerSystemContext _context;

        public DealerService(EVDealerSystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Dealer>> GetAllDealersAsync()
        {
            return await _context.Dealers
                .Where(d => d.IsActive == true)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Dealer?> GetDealerByIdAsync(Guid id)
        {
            return await _context.Dealers
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive == true);
        }
    }
}
