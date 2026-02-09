using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace WirexApp.Tests.Integration.Gateway
{
    public class GatewayWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Override services for testing
                // Example: Replace real Kafka with in-memory implementation
            });

            builder.UseEnvironment("Testing");
        }
    }
}
