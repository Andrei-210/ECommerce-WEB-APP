using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> CreateReviewAsync(int userId, int productId, CreateReviewDto dto);
    Task<IEnumerable<ReviewDto>> GetByProductIdAsync(int productId);
}
