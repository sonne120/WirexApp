using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WirexApp.Infrastructure.DataAccess
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _writeConnectionString;
        private readonly string _readConnectionString;
        private readonly ILogger<SqlConnectionFactory> _logger;

        public SqlConnectionFactory(IConfiguration configuration, ILogger<SqlConnectionFactory> logger)
        {
            _logger = logger;
            
            _writeConnectionString = configuration.GetConnectionString("WriteConnection")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Write connection string not found");
            
            _readConnectionString = configuration.GetConnectionString("ReadConnection")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Read connection string not found");

            _logger.LogInformation("SqlConnectionFactory initialized");
            _logger.LogDebug("Write connection configured: {HasWriteConnection}", !string.IsNullOrEmpty(_writeConnectionString));
            _logger.LogDebug("Read connection configured: {HasReadConnection}", !string.IsNullOrEmpty(_readConnectionString));
        }

        public IDbConnection CreateWriteConnection()
        {
            _logger.LogDebug("Creating write connection");

            var connection = new SqlConnection(_writeConnectionString);
            connection.Open();

            _logger.LogDebug("Write connection created and opened");

            return connection;
        }

        public IDbConnection CreateReadConnection()
        {
            _logger.LogDebug("Creating read connection");

            var connection = new SqlConnection(_readConnectionString);
            connection.Open();

            _logger.LogDebug("Read connection created and opened");

            return connection;
        }
    }
}
