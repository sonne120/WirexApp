using System.Data;

namespace WirexApp.Infrastructure.DataAccess
{
    /// <summary>
    /// Factory for creating database connections
    /// Supports separate read and write connection strings for CQRS optimization
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Creates a connection for write operations (commands)
        /// Points to the primary/master database
        /// </summary>
        IDbConnection CreateWriteConnection();

        /// <summary>
        /// Creates a connection for read operations (queries)
        /// Can point to read replicas or separate read-optimized database
        /// </summary>
        IDbConnection CreateReadConnection();
    }
}
