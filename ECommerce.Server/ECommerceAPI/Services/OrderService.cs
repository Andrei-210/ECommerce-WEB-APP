using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<OrderResponseDto> CheckoutAsync(int userId, CheckoutDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            throw new ArgumentException("Cart cannot be empty.");

        if (string.IsNullOrWhiteSpace(dto.ShippingAddress))
            throw new ArgumentException("Shipping address is required.");

        if (string.IsNullOrWhiteSpace(dto.City))
            throw new ArgumentException("City is required.");

        if (string.IsNullOrWhiteSpace(dto.PostalCode))
            throw new ArgumentException("Postal code is required.");

        var productIds = dto.Items.Select(i => i.ProductId).Distinct();
        var products = (await _productRepository.GetByIdsAsync(productIds))
            .ToDictionary(p => p.Id);

        var orderItems = new List<OrderItem>();
        decimal totalPrice = 0;

        foreach (var cartItem in dto.Items)
        {
            if (cartItem.Quantity <= 0)
                throw new ArgumentException($"Quantity must be greater than 0 for product {cartItem.ProductId}.");

            if (!products.TryGetValue(cartItem.ProductId, out var product))
                throw new KeyNotFoundException($"Product with ID {cartItem.ProductId} not found.");

            if (product.Stock < cartItem.Quantity)
                throw new InvalidOperationException($"Insufficient stock for '{product.Name}'. Available: {product.Stock}.");

            totalPrice += product.Price * cartItem.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = cartItem.Quantity
            });
        }

        var order = new Order
        {
            UserId = userId,
            TotalPrice = totalPrice,
            ShippingAddress = dto.ShippingAddress,
            City = dto.City,
            PostalCode = dto.PostalCode,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            Items = orderItems
        };

        // Creates order AND decrements stock atomically in a transaction
        var orderId = await _orderRepository.CreateOrderAsync(order);

        return new OrderResponseDto
        {
            OrderId = orderId,
            TotalPrice = totalPrice,
            Status = "Pending",
            CreatedAt = order.CreatedAt
        };
    }
}
