using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using WirexApp.Application.Payments.ReadModels;
using WirexApp.Gateway.Grpc;
using WirexApp.Infrastructure.DataAccess.Read;

namespace WirexApp.ReadService.Grpc
{
    public class PaymentReadGrpcService : Gateway.Grpc.PaymentReadService.PaymentReadServiceBase
    {
        private readonly IReadService<PaymentReadModel> _paymentReadService;
        private readonly ILogger<PaymentReadGrpcService> _logger;

        public PaymentReadGrpcService(
            IReadService<PaymentReadModel> paymentReadService,
            ILogger<PaymentReadGrpcService> logger)
        {
            _paymentReadService = paymentReadService;
            _logger = logger;
        }

        public override async Task<GetPaymentResponse> GetPayment(
            GetPaymentRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("[ReadService←gRPC] Received GetPayment for {PaymentId}", request.PaymentId);

            try
            {
                if (!Guid.TryParse(request.PaymentId, out var paymentId))
                {
                    _logger.LogWarning("[ReadService] Invalid PaymentId format");
                    return new GetPaymentResponse { Found = false };
                }

                var payment = await _paymentReadService.GetByIdAsync(paymentId, context.CancellationToken);

                if (payment == null)
                {
                    _logger.LogInformation("[ReadService→gRPC] Payment not found");
                    return new GetPaymentResponse { Found = false };
                }

                _logger.LogInformation("[ReadService→gRPC] Payment found and returned");

                return new GetPaymentResponse
                {
                    Found = true,
                    Payment = MapToGrpcModel(payment)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReadService] Error getting payment");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<GetAllPaymentsResponse> GetAllPayments(
            GetAllPaymentsRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("[ReadService←gRPC] Received GetAllPayments");

            try
            {
                var payments = await _paymentReadService.GetAllAsync(context.CancellationToken);

                var response = new GetAllPaymentsResponse();
                response.Payments.AddRange(payments.Select(MapToGrpcModel));

                _logger.LogInformation("[ReadService→gRPC] Returning {Count} payments", response.Payments.Count);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReadService] Error getting all payments");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<GetPaymentsByUserResponse> GetPaymentsByUser(
            GetPaymentsByUserRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("[ReadService←gRPC] Received GetPaymentsByUser for {UserId}", request.UserId);

            try
            {
                if (!Guid.TryParse(request.UserId, out var userId))
                {
                    return new GetPaymentsByUserResponse();
                }

                var payments = await _paymentReadService.FindAsync(
                    p => p.UserId == userId,
                    context.CancellationToken);

                var response = new GetPaymentsByUserResponse();
                response.Payments.AddRange(payments.Select(MapToGrpcModel));

                _logger.LogInformation("[ReadService→gRPC] Returning {Count} payments for user", response.Payments.Count);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReadService] Error getting user payments");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<GetPaymentStatsResponse> GetPaymentStats(
            GetPaymentStatsRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("[ReadService←gRPC] Received GetPaymentStats");

            try
            {
                var payments = await _paymentReadService.GetAllAsync(context.CancellationToken);
                var paymentsList = payments.ToList();

                var stats = new GetPaymentStatsResponse
                {
                    TotalPayments = paymentsList.Count,
                    TotalAmount = (double)paymentsList.Sum(p => p.SourceValue),
                    PendingPayments = paymentsList.Count(p => p.Status == "ToPay"),
                    CompletedPayments = paymentsList.Count(p => p.Status == "Completed"),
                    FailedPayments = paymentsList.Count(p => p.Status == "Failed")
                };

                _logger.LogInformation("[ReadService→gRPC] Returning stats: Total={Total}", stats.TotalPayments);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReadService] Error getting stats");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        private static PaymentModel MapToGrpcModel(PaymentReadModel model)
        {
            return new PaymentModel
            {
                PaymentId = model.PaymentId.ToString(),
                UserAccountId = model.UserAccountId.ToString(),
                UserId = model.UserId.ToString(),
                UserName = model.UserName ?? string.Empty,
                UserEmail = model.UserEmail ?? string.Empty,
                SourceCurrency = model.SourceCurrency ?? string.Empty,
                TargetCurrency = model.TargetCurrency ?? string.Empty,
                SourceValue = (double)model.SourceValue,
                TargetValue = (double)model.TargetValue,
                Status = model.Status ?? string.Empty,
                CreateDate = model.CreateDate.ToString("O"),
                IsRemoved = model.IsRemoved,
                ExchangeRate = (double)model.ExchangeRate,
                StatusDescription = model.StatusDescription ?? string.Empty
            };
        }
    }
}
