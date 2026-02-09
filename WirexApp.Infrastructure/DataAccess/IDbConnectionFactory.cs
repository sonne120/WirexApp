using System.Data;

namespace WirexApp.Infrastructure.DataAccess
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateWriteConnection();
        
        IDbConnection CreateReadConnection();
    }
}
