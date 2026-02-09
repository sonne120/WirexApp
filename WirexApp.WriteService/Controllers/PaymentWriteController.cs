using System;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using WirexApp.Application.Payments;

namespace WirexApp.WriteService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/payments")]
    public class PaymentWriteController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentWriteController> _logger;

        public PaymentWriteController(IMediator mediator, ILogger<PaymentWriteController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        
        [HttpPost("user/{userId}")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreatePayment(
            [FromRoute] Guid userId,
            [FromBody] CreatePaymentRequest request)
        {
            _logger.LogInformation("[WriteService] Creating payment for user {UserId}", userId);

            var command = new PaymentCreatedCommand(
                userId,
                request.SourceCurrency,
                request.TargetCurrency,
                request.SourceValue);

            await _mediator.Send(command);

            _logger.LogInformation("[WriteService] Payment command accepted for user {UserId}", userId);

            return Accepted(new
            {
                message = "Payment command accepted and will be processed",
                userId = userId
            });
        }
    }

    public class CreatePaymentRequest
    {
        public int SourceCurrency { get; set; }
        public int TargetCurrency { get; set; }
        public decimal SourceValue { get; set; }
    }
}
