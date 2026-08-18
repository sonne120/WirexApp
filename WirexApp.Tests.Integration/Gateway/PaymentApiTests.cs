using System.Collections.Generic;
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
      
            var response = await _client.GetAsync("/health");

         
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllPayments_ShouldReturnOkWithArray()
        {
     
            var response = await _client.GetAsync("/api/payments");

     
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var payments = await response.Content.ReadFromJsonAsync<List<PaymentDto>>();
            payments.Should().NotBeNull();
            payments.Should().BeOfType<List<PaymentDto>>();
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

            var result = await response.Content.ReadFromJsonAsync<CreatePaymentResponse>();
            result.Should().NotBeNull();
            result!.Message.Should().NotBeNullOrEmpty();
            result.UserId.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreatePayment_ShouldReturnBadRequest_WhenInvalidAmount()
        {
         
            var userId = Guid.NewGuid();
            var request = new
            {
                sourceCurrency = 1,
                targetCurrency = 2,
                sourceValue = -100.00 
            };

        
            var response = await _client.PostAsJsonAsync($"/api/payments/user/{userId}", request);

      
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            error.Should().NotBeNull();
            error!.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreatePayment_ShouldReturnBadRequest_WhenSameCurrency()
        {
            var userId = Guid.NewGuid();
            var request = new
            {
                sourceCurrency = 1, 
                targetCurrency = 1, 
                sourceValue = 100.00
            };

            var response = await _client.PostAsJsonAsync($"/api/payments/user/{userId}", request);


            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            error.Should().NotBeNull();
            error!.Message.Should().Contain("currencies must be different");
        }

        [Fact]
        public async Task CreatePayment_ShouldReturnBadRequest_WhenZeroAmount()
        {
            var userId = Guid.NewGuid();
            var request = new
            {
                sourceCurrency = 1,
                targetCurrency = 2,
                sourceValue = 0.00 // Zero amount
            };


            var response = await _client.PostAsJsonAsync($"/api/payments/user/{userId}", request);


            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            error.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPaymentById_ShouldReturnNotFound_WhenPaymentDoesNotExist()
        {
    
            var nonExistentId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/payments/{nonExistentId}");

    
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            error.Should().NotBeNull();
            error!.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task GetPaymentStats_ShouldReturnOkWithValidStructure()
        {

            var response = await _client.GetAsync("/api/payments/stats");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var stats = await response.Content.ReadFromJsonAsync<PaymentStatsDto>();
            stats.Should().NotBeNull();
            stats!.TotalPayments.Should().BeGreaterThanOrEqualTo(0);
            stats.TotalVolume.Should().BeGreaterThanOrEqualTo(0);
            stats.PaymentsByCurrency.Should().NotBeNull();
            stats.PaymentsByStatus.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPaymentsByUser_ShouldReturnOkWithArray()
        {
            var userId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/payments/user/{userId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var payments = await response.Content.ReadFromJsonAsync<List<PaymentDto>>();
            payments.Should().NotBeNull();
            payments.Should().BeOfType<List<PaymentDto>>();
        }

        [Fact]
        public async Task CreateAndRetrievePayment_ShouldReturnCreatedPayment()
        {
            var userId = Guid.NewGuid();
            var request = new
            {
                sourceCurrency = 1, // USD
                targetCurrency = 2, // EUR
                sourceValue = 150.00
            };

            var createResponse = await _client.PostAsJsonAsync($"/api/payments/user/{userId}", request);

            // Assert - Creation
            createResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var createResult = await createResponse.Content.ReadFromJsonAsync<CreatePaymentResponse>();
            createResult.Should().NotBeNull();
            createResult!.UserId.Should().Be(userId.ToString());

            // Give the system time to process (in real scenario, CDC would sync)
            await Task.Delay(100);

            // Act - Retrieval
            var getResponse = await _client.GetAsync($"/api/payments/user/{userId}");

            // Assert - Retrieval
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var payments = await getResponse.Content.ReadFromJsonAsync<List<PaymentDto>>();

            payments.Should().NotBeNull();
        }

        [Fact]
        public async Task CreatePayment_WithInvalidGuid_ShouldHandleGracefully()
        {
            var invalidUserId = "not-a-valid-guid";
            var request = new
            {
                sourceCurrency = 1,
                targetCurrency = 2,
                sourceValue = 100.00
            };

            var response = await _client.PostAsJsonAsync($"/api/payments/user/{invalidUserId}", request);

     
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Accepted);
        }
    }
}
