using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<int> CreateOrderAsync(Order order);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
}
