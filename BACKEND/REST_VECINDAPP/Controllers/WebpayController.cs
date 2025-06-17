using Microsoft.AspNetCore.Mvc;
using Transbank.Webpay.Common;
using Transbank.Webpay.WebpayPlus;
using Transbank.Webpay.WebpayPlus.Responses;
using Microsoft.Extensions.Configuration;
using Transbank.Common;
using REST_VECINDAPP.Servicios;

namespace REST_VECINDAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebpayController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebpayController> _logger;
        private readonly WebpayService _webpayService;

        public WebpayController(IConfiguration configuration, ILogger<WebpayController> logger, WebpayService webpayService)
        {
            _configuration = configuration;
            _logger = logger;
            _webpayService = webpayService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            try
            {
                var response = await _webpayService.IniciarTransaccion(request.BuyOrder, request.Amount);
                return Ok(new
                {
                    token = response.Token,
                    url = response.Url
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la transacción de WebPay");
                return StatusCode(500, new { error = "Error al procesar la transacción" });
            }
        }

        [HttpPost("commit")]
        public async Task<IActionResult> CommitTransaction([FromBody] CommitTransactionRequest request)
        {
            try
            {
                var response = await _webpayService.ConfirmarTransaccion(request.Token);
                return Ok(new
                {
                    amount = response.Amount,
                    status = response.Status,
                    buyOrder = response.BuyOrder,
                    sessionId = response.SessionId,
                    cardNumber = response.CardDetail?.CardNumber,
                    accountingDate = response.AccountingDate,
                    transactionDate = response.TransactionDate,
                    authorizationCode = response.AuthorizationCode,
                    paymentTypeCode = response.PaymentTypeCode,
                    responseCode = response.ResponseCode,
                    installmentsNumber = response.InstallmentsNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar la transacción de WebPay");
                return StatusCode(500, new { error = "Error al confirmar la transacción" });
            }
        }

        [HttpPost("confirmar")]
        public async Task<IActionResult> ConfirmarTransaccion([FromBody] CommitTransactionRequest request)
        {
            try
            {
                var response = await _webpayService.ConfirmarTransaccion(request.Token);
                return Ok(new
                {
                    amount = response.Amount,
                    status = response.Status,
                    buyOrder = response.BuyOrder,
                    sessionId = response.SessionId,
                    cardNumber = response.CardDetail?.CardNumber,
                    accountingDate = response.AccountingDate,
                    transactionDate = response.TransactionDate,
                    authorizationCode = response.AuthorizationCode,
                    paymentTypeCode = response.PaymentTypeCode,
                    responseCode = response.ResponseCode,
                    installmentsNumber = response.InstallmentsNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar la transacción de WebPay (confirmar)");
                return StatusCode(500, new { error = "Error al confirmar la transacción" });
            }
        }

        [HttpPost("status")]
        public async Task<IActionResult> GetTransactionStatus([FromBody] StatusRequest request)
        {
            try
            {
                var response = await _webpayService.ObtenerEstadoTransaccion(request.Token);
                return Ok(new
                {
                    amount = response.Amount,
                    status = response.Status,
                    buyOrder = response.BuyOrder,
                    sessionId = response.SessionId,
                    cardNumber = response.CardDetail?.CardNumber,
                    accountingDate = response.AccountingDate,
                    transactionDate = response.TransactionDate,
                    authorizationCode = response.AuthorizationCode,
                    paymentTypeCode = response.PaymentTypeCode,
                    responseCode = response.ResponseCode,
                    installmentsNumber = response.InstallmentsNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de la transacción de WebPay");
                return StatusCode(500, new { error = "Error al obtener el estado de la transacción" });
            }
        }
    }

    public class StatusRequest
    {
        public string Token { get; set; } = string.Empty;
    }
} 