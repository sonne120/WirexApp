using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WirexApp.Application.Payments.ReadModels;
using WirexApp.Infrastructure.DataAccess.Read;

namespace WirexApp.ReadService.Controllers
{

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/payments")]
    [ResponseCache(Duration = 60)] 
    public class PaymentReadController : ControllerBase
    {
        private readonly IReadService<PaymentReadModel> _paymentReadService;
        private readonly ILogger<PaymentReadController> _logger;

        public PaymentReadController(
            IReadService<PaymentReadModel> paymentReadService,
            ILogger<PaymentReadController> logger)
        {
            _paymentReadService = paymentReadService;
            _logger = logger;
        }
        
        [HttpGet("{paymentId}")]
        [ProducesResponseType(typeof(PaymentReadModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetPayment([FromRoute] Guid paymentId)
        {
            _logger.LogInformation("[ReadService] Getting payment {PaymentId}", paymentId);

            var payment = await _paymentReadService.GetByIdAsync(paymentId);

            if (payment == null)
            {
                _logger.LogWarning("[ReadService] Payment {PaymentId} not found", paymentId);
                return NotFound(new { message = "Payment not found" });
            }

            _logger.LogInformation("[ReadService] Payment {PaymentId} retrieved successfully", paymentId);
            return Ok(payment);
        }

 
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PaymentReadModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllPayments()
        {
            _logger.LogInformation("[ReadService] Getting all payments");

            var payments = await _paymentReadService.GetAllAsync();

            _logger.LogInformation("[ReadService] Retrieved {Count} payments", payments.Count());
            return Ok(payments);
        }

 
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<PaymentReadModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPaymentsByUser([FromRoute] Guid userId)
        {
            _logger.LogInformation("[ReadService] Getting payments for user {UserId}", userId);

            var payments = await _paymentReadService.FindAsync(p => p.UserId == userId);

            _logger.LogInformation("[ReadService] Retrieved {Count} payments for user {UserId}",
                payments.Count(), userId);

            return Ok(payments);
        }
        
        [HttpGet("stats")]
        [ProducesResponseType(typeof(PaymentStatistics), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPaymentStatistics()
        {
            _logger.LogInformation("[ReadService] Getting payment statistics");

            var allPayments = await _paymentReadService.GetAllAsync();
            var paymentsList = allPayments.ToList();

            var stats = new PaymentStatistics
            {
                TotalPayments = paymentsList.Count,
                TotalAmount = paymentsList.Sum(p => p.SourceValue),
                PendingPayments = paymentsList.Count(p => p.Status == "ToPay"),
                CompletedPayments = paymentsList.Count(p => p.Status == "Completed"),
                FailedPayments = paymentsList.Count(p => p.Status == "Failed")
            };

            _logger.LogInformation("[ReadService] Statistics: {Stats}", stats);
            return Ok(stats);
        }
    }

    public class PaymentStatistics
    {
        public int TotalPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public int PendingPayments { get; set; }
        public int CompletedPayments { get; set; }
        public int FailedPayments { get; set; }
    }
}
