using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace WirexApp.Tests.Integration.Gateway
{
    public class PaymentApiTests : IClassFixture<GatewayWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public PaymentApiTests(GatewayWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task HealthCheck_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllPayments_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/payments");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreatePayment_ShouldReturnAccepted_WhenValidRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new
            {
                sourceCurrency = 1, // USD
                targetCurrency = 2, // EUR
                sourceValue = 100.00
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/payments/user/{userId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task CreatePayment_ShouldReturnBadRequest_WhenInvalidAmount()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new
            {
                sourceCurrency = 1,
                targetCurrency = 2,
                sourceValue = -100.00 // Invalid negative amount
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/payments/user/{userId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetPaymentById_ShouldReturnNotFound_WhenPaymentDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/payments/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPaymentStats_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/payments/stats");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
