using Moq;
using FluentAssertions;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services;

namespace ECommerceAPI.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _orderService = new OrderService(_orderRepoMock.Object, _productRepoMock.Object);
    }

    [Fact]
    public async Task CheckoutAsync_CalculatesTotalFromDB_NotFromClient()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Dell XPS 15", Price = 5999.99m, Stock = 10 },
            new() { Id = 2, Name = "Logitech Mouse", Price = 349.99m, Stock = 20 }
        };

        _productRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(products);
        _orderRepoMock
            .Setup(r => r.CreateOrderAsync(It.IsAny<Order>()))
            .ReturnsAsync(1);

        var dto = new CheckoutDto
        {
            ShippingAddress = "Str. Libertatii 1",
            City = "Craiova",
            PostalCode = "200001",
            Items = new List<CartItemDto>
            {
                new() { ProductId = 1, Quantity = 1 },
                new() { ProductId = 2, Quantity = 2 }
            }
        };

        // Act
        var result = await _orderService.CheckoutAsync(userId: 1, dto);

        // Assert — total must be server-side: 5999.99 + 349.99*2 = 6699.97
        result.TotalPrice.Should().Be(6699.97m);
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task CheckoutAsync_EmptyCart_ThrowsArgumentException()
    {
        var dto = new CheckoutDto
        {
            ShippingAddress = "Str. Test 1",
            City = "Bucuresti",
            PostalCode = "010001",
            Items = new List<CartItemDto>()
        };

        var act = async () => await _orderService.CheckoutAsync(1, dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*empty*");
    }

    [Fact]
    public async Task CheckoutAsync_MissingShippingAddress_ThrowsArgumentException()
    {
        var dto = new CheckoutDto
        {
            ShippingAddress = "",
            City = "Cluj",
            PostalCode = "400001",
            Items = new List<CartItemDto> { new() { ProductId = 1, Quantity = 1 } }
        };

        var act = async () => await _orderService.CheckoutAsync(1, dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*address*");
    }

    [Fact]
    public async Task CheckoutAsync_ProductNotFound_ThrowsKeyNotFoundException()
    {
        _productRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Product>());

        var dto = new CheckoutDto
        {
            ShippingAddress = "Str. Test 1",
            City = "Timisoara",
            PostalCode = "300001",
            Items = new List<CartItemDto> { new() { ProductId = 999, Quantity = 1 } }
        };

        var act = async () => await _orderService.CheckoutAsync(1, dto);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CheckoutAsync_InsufficientStock_ThrowsInvalidOperationException()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Cisco Switch", Price = 1899.99m, Stock = 2 }
        };

        _productRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(products);

        var dto = new CheckoutDto
        {
            ShippingAddress = "Str. Test 5",
            City = "Iasi",
            PostalCode = "700001",
            Items = new List<CartItemDto> { new() { ProductId = 1, Quantity = 10 } }
        };

        var act = async () => await _orderService.CheckoutAsync(1, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*stock*");
    }

    [Fact]
    public async Task CheckoutAsync_ZeroQuantity_ThrowsArgumentException()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Samsung SSD", Price = 399.99m, Stock = 10 }
        };

        _productRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(products);

        var dto = new CheckoutDto
        {
            ShippingAddress = "Str. Test 1",
            City = "Brasov",
            PostalCode = "500001",
            Items = new List<CartItemDto> { new() { ProductId = 1, Quantity = 0 } }
        };

        var act = async () => await _orderService.CheckoutAsync(1, dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Quantity*");
    }
}
