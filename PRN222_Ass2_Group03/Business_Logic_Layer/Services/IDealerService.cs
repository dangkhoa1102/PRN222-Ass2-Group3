using Business_Logic_Layer.DTOs;

namespace Business_Logic_Layer.Services
{
    public interface IDealerService
    {
        Task<IEnumerable<DealerDTO>> GetAllDealersAsync();
        Task<DealerDTO?> GetDealerByIdAsync(Guid id);
    }
}