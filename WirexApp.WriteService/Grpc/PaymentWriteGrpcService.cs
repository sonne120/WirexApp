using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WirexApp.Application.Payments;
using WirexApp.Gateway.Grpc;

namespace WirexApp.WriteService.Grpc
{
    
    public class PaymentWriteGrpcService : PaymentWriteService.PaymentWriteServiceBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentWriteGrpcService> _logger;

        public PaymentWriteGrpcService(IMediator mediator, ILogger<PaymentWriteGrpcService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override async Task<CreatePaymentResponse> CreatePayment(
            CreatePaymentRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("[WriteService←gRPC] Received CreatePayment for user {UserId}", request.UserId);

            try
            {
                if (!Guid.TryParse(request.UserId, out var userId))
                {
                    _logger.LogWarning("[WriteService] Invalid UserId format: {UserId}", request.UserId);
                    return new CreatePaymentResponse
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    };
                }

                var command = new PaymentCreatedCommand(
                    userId,
                    (Domain.Currency)request.SourceCurrency,
                    (Domain.Currency)request.TargetCurrency,
                    (decimal)request.SourceValue);

                await _mediator.Send(command, context.CancellationToken);

                _logger.LogInformation("[WriteService→gRPC] Payment command processed successfully");

                return new CreatePaymentResponse
                {
                    Success = true,
                    Message = "Payment command accepted and will be processed",
                    UserId = request.UserId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WriteService] Error processing CreatePayment");

                return new CreatePaymentResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}
