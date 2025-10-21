using EVDealerDbContext.Models;

namespace Business_Logic_Layer.Services
{
    public interface IDealerService
    {
        Task<IEnumerable<Dealer>> GetAllDealersAsync();
        Task<Dealer?> GetDealerByIdAsync(Guid id);
    }
}
