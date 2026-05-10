using Microsoft.Data.SqlClient;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;

namespace ECommerceAPI.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT Id, Username, Email, PasswordHash, CreatedAt FROM Users WHERE Email = @Email";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapUser(reader);

        return null;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT Id, Username, Email, PasswordHash, CreatedAt FROM Users WHERE Id = @Id";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapUser(reader);

        return null;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);

        var count = (int)(await command.ExecuteScalarAsync() ?? 0);
        return count > 0;
    }

    public async Task<int> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO Users (Username, Email, PasswordHash, CreatedAt)
            VALUES (@Username, @Email, @PasswordHash, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private static User MapUser(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Username = reader.GetString(1),
        Email = reader.GetString(2),
        PasswordHash = reader.GetString(3),
        CreatedAt = reader.GetDateTime(4)
    };
}
