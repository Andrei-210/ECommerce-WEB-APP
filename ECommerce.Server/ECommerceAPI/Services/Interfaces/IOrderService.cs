using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CheckoutAsync(int userId, CheckoutDto dto);
}
