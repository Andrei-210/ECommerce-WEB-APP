using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories.Interfaces;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}
