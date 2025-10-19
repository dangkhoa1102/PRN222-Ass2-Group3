using EVDealerDbContext.Models;

namespace DataAccess_Layer.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAll();
        Task<Order?> GetById(Guid id);
        Task<bool> Add(Order order);
        Task<bool> Update(Order order);
        Task<bool> Delete(Guid id);
        Task<List<Order>> GetByCustomerId(Guid customerId);
        Task<List<Order>> GetByStatus(string status);
        Task<bool> AddOrderHistory(Guid orderId, string status, string notes, Guid createdBy);
    }
}
