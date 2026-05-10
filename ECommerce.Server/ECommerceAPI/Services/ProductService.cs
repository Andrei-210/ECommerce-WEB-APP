using ECommerceAPI.DTOs;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            Category = p.Category,
            Brand = p.Brand,
            ImageUrl = p.ImageUrl,
            Specifications = p.Specifications.Select(s => new ProductSpecificationDto
            {
                Key = s.SpecKey,
                Value = s.SpecValue
            }).ToList(),
            Reviews = p.Reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = r.Username,
                Rating = r.Rating,
                Title = r.Title,
                Body = r.Body,
                CreatedAt = r.CreatedAt
            }).ToList(),
            AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = p.Reviews.Count
        });
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return null;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            Category = product.Category,
            Brand = product.Brand,
            ImageUrl = product.ImageUrl,
            Specifications = product.Specifications.Select(s => new ProductSpecificationDto
            {
                Key = s.SpecKey,
                Value = s.SpecValue
            }).ToList(),
            Reviews = product.Reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = r.Username,
                Rating = r.Rating,
                Title = r.Title,
                Body = r.Body,
                CreatedAt = r.CreatedAt
            }).ToList(),
            AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = product.Reviews.Count
        };
    }
}
