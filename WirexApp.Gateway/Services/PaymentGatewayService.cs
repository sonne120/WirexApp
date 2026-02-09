using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WirexApp.Gateway.Grpc;

namespace WirexApp.Gateway.Services
{
    /// <summary>
    /// Gateway service that communicates with Write/Read services via gRPC
    /// </summary>
    public class PaymentGatewayService
    {
        private readonly PaymentWriteService.PaymentWriteServiceClient _writeClient;
        private readonly PaymentReadService.PaymentReadServiceClient _readClient;
        private readonly ILogger<PaymentGatewayService> _logger;

        public PaymentGatewayService(
            PaymentWriteService.PaymentWriteServiceClient writeClient,
            PaymentReadService.PaymentReadServiceClient readClient,
            ILogger<PaymentGatewayService> logger)
        {
            _writeClient = writeClient;
            _readClient = readClient;
            _logger = logger;
        }

        public async Task<CreatePaymentResponse> CreatePaymentAsync(
            string userId,
            int sourceCurrency,
            int targetCurrency,
            double sourceValue)
        {
            try
            {
                _logger.LogInformation("[Gateway→gRPC] Creating payment for user {UserId}", userId);

                var request = new CreatePaymentRequest
                {
                    UserId = userId,
                    SourceCurrency = sourceCurrency,
                    TargetCurrency = targetCurrency,
                    SourceValue = sourceValue
                };

                var response = await _writeClient.CreatePaymentAsync(request);

                _logger.LogInformation("[Gateway←gRPC] Payment creation response: {Success}", response.Success);

                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "[Gateway] gRPC error creating payment");
                throw;
            }
        }

        public async Task<PaymentModel?> GetPaymentAsync(string paymentId)
        {
            try
            {
                _logger.LogInformation("[Gateway→gRPC] Getting payment {PaymentId}", paymentId);

                var request = new GetPaymentRequest { PaymentId = paymentId };
                var response = await _readClient.GetPaymentAsync(request);

                if (!response.Found)
                {
                    _logger.LogWarning("[Gateway←gRPC] Payment {PaymentId} not found", paymentId);
                    return null;
                }

                _logger.LogInformation("[Gateway←gRPC] Payment {PaymentId} retrieved", paymentId);
                return response.Payment;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "[Gateway] gRPC error getting payment");
                throw;
            }
        }

        public async Task<List<PaymentModel>> GetAllPaymentsAsync()
        {
            try
            {
                _logger.LogInformation("[Gateway→gRPC] Getting all payments");

                var request = new GetAllPaymentsRequest();
                var response = await _readClient.GetAllPaymentsAsync(request);

                _logger.LogInformation("[Gateway←gRPC] Retrieved {Count} payments", response.Payments.Count);
                return response.Payments.ToList();
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "[Gateway] gRPC error getting all payments");
                throw;
            }
        }

        public async Task<List<PaymentModel>> GetPaymentsByUserAsync(string userId)
        {
            try
            {
                _logger.LogInformation("[Gateway→gRPC] Getting payments for user {UserId}", userId);

                var request = new GetPaymentsByUserRequest { UserId = userId };
                var response = await _readClient.GetPaymentsByUserAsync(request);

                _logger.LogInformation("[Gateway←gRPC] Retrieved {Count} payments for user", response.Payments.Count);
                return response.Payments.ToList();
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "[Gateway] gRPC error getting user payments");
                throw;
            }
        }

        public async Task<GetPaymentStatsResponse> GetPaymentStatsAsync()
        {
            try
            {
                _logger.LogInformation("[Gateway→gRPC] Getting payment statistics");

                var request = new GetPaymentStatsRequest();
                var response = await _readClient.GetPaymentStatsAsync(request);

                _logger.LogInformation("[Gateway←gRPC] Stats: Total={Total}, Pending={Pending}",
                    response.TotalPayments, response.PendingPayments);

                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "[Gateway] gRPC error getting stats");
                throw;
            }
        }
    }
}
