using Dto.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PaymentService.Application.Services;
using PaymentService.EndPoint.Models.Dtos;
using PaymentService.Infrastructure.MessagingBus.Messages;
using PaymentService.Infrastructure.MessagingBus.SendMessages;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZarinPal.Class;

namespace PaymentService.EndPoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayController : ControllerBase
    {
        private readonly ZarinPal.Class.Payment _payment;
        private readonly Authority _authority;
        private readonly Transactions _transactions;
        private readonly IPaymentService paymentService;
        private readonly IConfiguration configuration;
        private readonly IMessageBus messageBus;
        private readonly string merchendId;
        private readonly string QueueName_PaymentDone;
        public PayController(IPaymentService paymentService
            , IConfiguration configuration, IMessageBus messageBus)
        {
            var expose = new Expose();
            _payment = expose.CreatePayment();
            _authority = expose.CreateAuthority();
            _transactions = expose.CreateTransactions();
            this.paymentService = paymentService;
            this.configuration = configuration;
            this.messageBus = messageBus;
            merchendId = configuration["merchendId"];
            QueueName_PaymentDone = configuration["RabbitMq:QueueName_PaymentDone"];
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid OrderId, string callbackUrlFront)
        {
            var pay = paymentService.GetPaymentofOrder(OrderId);
            if (pay == null)
            {
                ResultDto resultDto = new ResultDto
                {
                    IsSuccess = false,
                    Message = "پرداخت یافت نشد"
                };
                return Ok(resultDto);
            }
            string callbackUrl = Url.Action(nameof(Verify), "pay",
                new
                {
                    paymentId = pay.PaymentId,
                    callbackUrlFront = callbackUrlFront
                },
                protocol: Request.Scheme);

            var result = await _payment.Request(new DtoRequest()
            {
                Amount = pay.Amount,
                CallbackUrl = callbackUrl,
                Description = "test",
                Email = "",
                Mobile = "",
                MerchantId = merchendId,
            }, ZarinPal.Class.Payment.Mode.zarinpal);

            string redirectUrl = $"https://zarinpal.com/pg/StartPay/{result.Authority}";

            return Ok(new ResultDto<ReturnPaymentLinkDto>
            {
                IsSuccess = true,
                Data = new ReturnPaymentLinkDto { PaymentLink = redirectUrl },
            });

        }


        [AllowAnonymous]
        [HttpGet("Verify")]
        public IActionResult Verify(Guid paymentId, string callbackUrlFront)
        {

            string Status = HttpContext.Request.Query["Status"];
            string Authority = HttpContext.Request.Query["authority"];

            if (Status != "" &&
                  Status.ToString().ToLower() == "ok" &&
                 Authority != "")
            {
                var pay = paymentService.GetPayment(paymentId);
                if (pay == null)
                {
                    return NotFound();
                }

                var client = new RestClient("https://www.zarinpal.com/pg/rest/WebGate/PaymentVerification.json");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", $"{{\"MerchantID\" :\"{merchendId}\",\"Authority\":\"{Authority}\",\"Amount\":\"{pay.Amount}\"}}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                VerificationPayResultDto verification = JsonConvert.DeserializeObject<VerificationPayResultDto>(response.Content);

                if (verification.Status == 100)
                {
                    paymentService.PayDone(paymentId, Authority, verification.RefID);

                    // ارسال پیغام برای سرویس سفارش

                    PaymentIsDoneMessage paymentIsDoneMessage = new PaymentIsDoneMessage
                    {
                        Creationtime = DateTime.Now,
                        MessageId = Guid.NewGuid(),
                        OrderId = pay.OrderId,
                    };
                    messageBus.SendMessage(paymentIsDoneMessage, QueueName_PaymentDone);

                    return Redirect(callbackUrlFront);

                }
                else
                {
                    return NotFound(callbackUrlFront);
                }
            }


            return Redirect(callbackUrlFront);
        }
    }
}
