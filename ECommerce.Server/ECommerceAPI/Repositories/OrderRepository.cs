using Microsoft.Data.SqlClient;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;

namespace ECommerceAPI.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateOrderAsync(Order order)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Insert order
            const string orderSql = @"
                INSERT INTO Orders (UserId, TotalPrice, ShippingAddress, City, PostalCode, Status, CreatedAt)
                VALUES (@UserId, @TotalPrice, @ShippingAddress, @City, @PostalCode, @Status, @CreatedAt);
                SELECT SCOPE_IDENTITY();";

            int orderId;
            using (var cmd = new SqlCommand(orderSql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@UserId", order.UserId);
                cmd.Parameters.AddWithValue("@TotalPrice", order.TotalPrice);
                cmd.Parameters.AddWithValue("@ShippingAddress", order.ShippingAddress);
                cmd.Parameters.AddWithValue("@City", order.City);
                cmd.Parameters.AddWithValue("@PostalCode", order.PostalCode);
                cmd.Parameters.AddWithValue("@Status", order.Status);
                cmd.Parameters.AddWithValue("@CreatedAt", order.CreatedAt);

                var result = await cmd.ExecuteScalarAsync();
                orderId = Convert.ToInt32(result);
            }

            // Insert order items + decrement stock atomically
            const string itemSql = @"
                INSERT INTO OrderItems (OrderId, ProductId, ProductName, UnitPrice, Quantity)
                VALUES (@OrderId, @ProductId, @ProductName, @UnitPrice, @Quantity)";

            const string stockSql = @"
                UPDATE Products
                SET Stock = Stock - @Quantity
                WHERE Id = @ProductId AND Stock >= @Quantity";

            foreach (var item in order.Items)
            {
                using (var itemCmd = new SqlCommand(itemSql, connection, transaction))
                {
                    itemCmd.Parameters.AddWithValue("@OrderId", orderId);
                    itemCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                    itemCmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                    itemCmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                    itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    await itemCmd.ExecuteNonQueryAsync();
                }

                // Decrement stock — the WHERE Stock >= @Quantity prevents going negative
                using (var stockCmd = new SqlCommand(stockSql, connection, transaction))
                {
                    stockCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    stockCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                    var rowsAffected = await stockCmd.ExecuteNonQueryAsync();

                    // If 0 rows affected, stock was insufficient (race condition) — rollback
                    if (rowsAffected == 0)
                        throw new InvalidOperationException($"Insufficient stock for product ID {item.ProductId}.");
                }
            }

            await transaction.CommitAsync();
            return orderId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        var orders = new Dictionary<int, Order>();

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT o.Id, o.UserId, o.TotalPrice, o.ShippingAddress, o.City, o.PostalCode, o.Status, o.CreatedAt,
                   oi.Id, oi.ProductId, oi.ProductName, oi.UnitPrice, oi.Quantity
            FROM Orders o
            LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
            WHERE o.UserId = @UserId
            ORDER BY o.CreatedAt DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var orderId = reader.GetInt32(0);
            if (!orders.TryGetValue(orderId, out var order))
            {
                order = new Order
                {
                    Id = orderId,
                    UserId = reader.GetInt32(1),
                    TotalPrice = reader.GetDecimal(2),
                    ShippingAddress = reader.GetString(3),
                    City = reader.GetString(4),
                    PostalCode = reader.GetString(5),
                    Status = reader.GetString(6),
                    CreatedAt = reader.GetDateTime(7),
                    Items = new List<OrderItem>()
                };
                orders[orderId] = order;
            }

            if (!reader.IsDBNull(8))
            {
                order.Items.Add(new OrderItem
                {
                    Id = reader.GetInt32(8),
                    OrderId = orderId,
                    ProductId = reader.GetInt32(9),
                    ProductName = reader.GetString(10),
                    UnitPrice = reader.GetDecimal(11),
                    Quantity = reader.GetInt32(12)
                });
            }
        }

        return orders.Values;
    }
}
