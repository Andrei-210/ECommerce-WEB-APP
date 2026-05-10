using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories.Interfaces;

public interface IReviewRepository
{
    Task<IEnumerable<Review>> GetByProductIdAsync(int productId);
    Task<Review?> GetByUserAndProductAsync(int userId, int productId);
    Task<int> CreateAsync(Review review);
}
