using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using WirexApp.Gateway.Services;

namespace WirexApp.Gateway.Controllers
{

    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentGatewayService _gatewayService;

        public PaymentController(PaymentGatewayService gatewayService)
        {
            _gatewayService = gatewayService;
        }
        
        [HttpPost("user/{userId}")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreatePayment(
            [FromRoute] string userId,
            [FromBody] CreatePaymentRequestDto request)
        {
            var response = await _gatewayService.CreatePaymentAsync(
                userId,
                request.SourceCurrency,
                request.TargetCurrency,
                request.SourceValue);

            if (response.Success)
            {
                return Accepted(new
                {
                    message = response.Message,
                    userId = response.UserId
                });
            }

            return BadRequest(new { message = response.Message });
        }
        
        [HttpGet("{paymentId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetPayment([FromRoute] string paymentId)
        {
            var payment = await _gatewayService.GetPaymentAsync(paymentId);

            if (payment == null)
            {
                return NotFound(new { message = "Payment not found" });
            }

            return Ok(payment);
        }
        
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _gatewayService.GetAllPaymentsAsync();
            return Ok(payments);
        }
        
        [HttpGet("user/{userId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPaymentsByUser([FromRoute] string userId)
        {
            var payments = await _gatewayService.GetPaymentsByUserAsync(userId);
            return Ok(payments);
        }
        
        [HttpGet("stats")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPaymentStats()
        {
            var stats = await _gatewayService.GetPaymentStatsAsync();
            return Ok(stats);
        }
    }

    public class CreatePaymentRequestDto
    {
        public int SourceCurrency { get; set; }
        public int TargetCurrency { get; set; }
        public double SourceValue { get; set; }
    }
}
