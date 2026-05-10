using Microsoft.Data.SqlClient;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;

namespace ECommerceAPI.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReviewRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId)
    {
        var reviews = new List<Review>();
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT r.Id, r.UserId, u.Username, r.Rating, r.Title, r.Body, r.CreatedAt
            FROM Reviews r
            JOIN Users u ON r.UserId = u.Id
            WHERE r.ProductId = @ProductId
            ORDER BY r.CreatedAt DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ProductId", productId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reviews.Add(new Review
            {
                Id = reader.GetInt32(0),
                ProductId = productId,
                UserId = reader.GetInt32(1),
                Username = reader.GetString(2),
                Rating = reader.GetInt32(3),
                Title = reader.GetString(4),
                Body = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                CreatedAt = reader.GetDateTime(6)
            });
        }

        return reviews;
    }

    public async Task<Review?> GetByUserAndProductAsync(int userId, int productId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            SELECT r.Id, r.UserId, u.Username, r.Rating, r.Title, r.Body, r.CreatedAt
            FROM Reviews r
            JOIN Users u ON r.UserId = u.Id
            WHERE r.UserId = @UserId AND r.ProductId = @ProductId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@ProductId", productId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Review
            {
                Id = reader.GetInt32(0),
                ProductId = productId,
                UserId = reader.GetInt32(1),
                Username = reader.GetString(2),
                Rating = reader.GetInt32(3),
                Title = reader.GetString(4),
                Body = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                CreatedAt = reader.GetDateTime(6)
            };
        }

        return null;
    }

    public async Task<int> CreateAsync(Review review)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO Reviews (ProductId, UserId, Rating, Title, Body, CreatedAt)
            VALUES (@ProductId, @UserId, @Rating, @Title, @Body, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ProductId", review.ProductId);
        command.Parameters.AddWithValue("@UserId", review.UserId);
        command.Parameters.AddWithValue("@Rating", review.Rating);
        command.Parameters.AddWithValue("@Title", review.Title);
        command.Parameters.AddWithValue("@Body", review.Body);
        command.Parameters.AddWithValue("@CreatedAt", review.CreatedAt);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
