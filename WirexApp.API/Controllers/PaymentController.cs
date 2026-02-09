using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Net;
using WirexApp.API.Models;
using WirexApp.Application.Payments;
using WirexApp.Domain;

namespace WirexApp.API.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase 
    {

        readonly IMediator _mediator;
        public PaymentController(IMediator mediator)
        {
            this._mediator = mediator;
        }


        [Route("{userId}")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<IActionResult> AddPayment([FromRoute]Guid userId,[FromBody]PaymentData request)
        {
            await _mediator.Send(new PaymentCreatedCommand(userId,request.sourceCurrency, request.targetCurrency,request.sourceValue));

            return Created(string.Empty, null);
        }
    }
}

