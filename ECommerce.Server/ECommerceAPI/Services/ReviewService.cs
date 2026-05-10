using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;

    public ReviewService(IReviewRepository reviewRepository, IProductRepository productRepository)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
    }

    public async Task<ReviewDto> CreateReviewAsync(int userId, int productId, CreateReviewDto dto)
    {
        if (dto.Rating < 1 || dto.Rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.");

        if (string.IsNullOrWhiteSpace(dto.Title) || dto.Title.Length < 3)
            throw new ArgumentException("Review title must be at least 3 characters.");

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new KeyNotFoundException($"Product {productId} not found.");

        var existing = await _reviewRepository.GetByUserAndProductAsync(userId, productId);
        if (existing != null)
            throw new InvalidOperationException("You have already reviewed this product.");

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = dto.Rating,
            Title = dto.Title.Trim(),
            Body = dto.Body.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var id = await _reviewRepository.CreateAsync(review);
        review.Id = id;

        return MapToDto(review);
    }

    public async Task<IEnumerable<ReviewDto>> GetByProductIdAsync(int productId)
    {
        var reviews = await _reviewRepository.GetByProductIdAsync(productId);
        return reviews.Select(MapToDto);
    }

    private static ReviewDto MapToDto(Review r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        Username = r.Username,
        Rating = r.Rating,
        Title = r.Title,
        Body = r.Body,
        CreatedAt = r.CreatedAt
    };
}
