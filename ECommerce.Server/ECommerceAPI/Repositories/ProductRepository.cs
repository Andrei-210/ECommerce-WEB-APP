using Microsoft.Data.SqlClient;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;

namespace ECommerceAPI.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        var products = new Dictionary<int, Product>();
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        // Load products + specs
        const string productSql = @"
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock,
                   p.Category, p.Brand, p.ImageUrl, p.CreatedAt,
                   ps.SpecKey, ps.SpecValue
            FROM Products p
            LEFT JOIN ProductSpecifications ps ON p.Id = ps.ProductId
            ORDER BY p.Id, ps.Id";

        using (var cmd = new SqlCommand(productSql, connection))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var id = reader.GetInt32(0);
                if (!products.TryGetValue(id, out var product))
                {
                    product = MapProductBase(reader);
                    products[id] = product;
                }

                if (!reader.IsDBNull(9))
                {
                    var key = reader.GetString(9);
                    if (!product.Specifications.Any(s => s.SpecKey == key))
                    {
                        product.Specifications.Add(new ProductSpecification
                        {
                            SpecKey = key,
                            SpecValue = reader.GetString(10)
                        });
                    }
                }
            }
        }

        if (!products.Any()) return products.Values;

        // Load reviews for all products in one query
        const string reviewSql = @"
            SELECT r.Id, r.ProductId, r.UserId, u.Username, r.Rating, r.Title, r.Body, r.CreatedAt
            FROM Reviews r
            JOIN Users u ON r.UserId = u.Id
            ORDER BY r.ProductId, r.CreatedAt DESC";

        using (var cmd = new SqlCommand(reviewSql, connection))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var productId = reader.GetInt32(1);
                if (!products.TryGetValue(productId, out var product)) continue;

                product.Reviews.Add(new Review
                {
                    Id = reader.GetInt32(0),
                    ProductId = productId,
                    UserId = reader.GetInt32(2),
                    Username = reader.GetString(3),
                    Rating = reader.GetInt32(4),
                    Title = reader.GetString(5),
                    Body = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    CreatedAt = reader.GetDateTime(7)
                });
            }
        }

        return products.Values;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string productSql = @"
            SELECT p.Id, p.Name, p.Description, p.Price, p.Stock,
                   p.Category, p.Brand, p.ImageUrl, p.CreatedAt,
                   ps.Id AS SpecId, ps.SpecKey, ps.SpecValue
            FROM Products p
            LEFT JOIN ProductSpecifications ps ON p.Id = ps.ProductId
            WHERE p.Id = @Id
            ORDER BY ps.Id";

        Product? product = null;

        using (var cmd = new SqlCommand(productSql, connection))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (product == null)
                    product = MapProductBase(reader);

                if (!reader.IsDBNull(9))
                {
                    product.Specifications.Add(new ProductSpecification
                    {
                        Id = reader.GetInt32(9),
                        ProductId = id,
                        SpecKey = reader.GetString(10),
                        SpecValue = reader.GetString(11)
                    });
                }
            }
        }

        if (product == null) return null;

        const string reviewSql = @"
            SELECT r.Id, r.UserId, u.Username, r.Rating, r.Title, r.Body, r.CreatedAt
            FROM Reviews r
            JOIN Users u ON r.UserId = u.Id
            WHERE r.ProductId = @ProductId
            ORDER BY r.CreatedAt DESC";

        using (var cmd = new SqlCommand(reviewSql, connection))
        {
            cmd.Parameters.AddWithValue("@ProductId", id);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                product.Reviews.Add(new Review
                {
                    Id = reader.GetInt32(0),
                    ProductId = id,
                    UserId = reader.GetInt32(1),
                    Username = reader.GetString(2),
                    Rating = reader.GetInt32(3),
                    Title = reader.GetString(4),
                    Body = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    CreatedAt = reader.GetDateTime(6)
                });
            }
        }

        return product;
    }

    public async Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.ToList();
        if (!idList.Any()) return Enumerable.Empty<Product>();

        var products = new List<Product>();
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var paramNames = idList.Select((_, i) => $"@Id{i}").ToList();
        var sql = $"SELECT Id, Name, Description, Price, Stock, Category, Brand, ImageUrl, CreatedAt FROM Products WHERE Id IN ({string.Join(",", paramNames)})";

        using var command = new SqlCommand(sql, connection);
        for (int i = 0; i < idList.Count; i++)
            command.Parameters.AddWithValue($"@Id{i}", idList[i]);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            products.Add(MapProductBase(reader));

        return products;
    }

    private static Product MapProductBase(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Description = reader.GetString(2),
        Price = reader.GetDecimal(3),
        Stock = reader.GetInt32(4),
        Category = reader.GetString(5),
        Brand = reader.GetString(6),
        ImageUrl = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
        CreatedAt = reader.GetDateTime(8)
    };
}
