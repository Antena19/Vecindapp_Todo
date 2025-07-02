using Transbank.Webpay.Common;
using Transbank.Webpay.WebpayPlus;
using Transbank.Webpay.WebpayPlus.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Transbank.Common;

namespace REST_VECINDAPP.Servicios
{
    public class WebpayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebpayService> _logger;
        private readonly Transaction _transaction;

        public WebpayService(IConfiguration configuration, ILogger<WebpayService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var options = new Options(
                _configuration["Transbank:CommerceCode"],
                _configuration["Transbank:ApiKey"],
                WebpayIntegrationType.Test
            );
            _transaction = new Transaction(options);
        }

        public async Task<CreateResponse> IniciarTransaccion(string ordenCompra, decimal monto)
        {
            try
            {
                var sessionId = Guid.NewGuid().ToString();
                var returnUrl = "https://backendvecindapp-production.up.railway.app/api/Webpay/commit";

                var response = await Task.Run(() => _transaction.Create(
                    buyOrder: ordenCompra,
                    sessionId: sessionId,
                    amount: monto,
                    returnUrl: returnUrl
                ));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar la transacción de WebPay");
                throw;
            }
        }

        public async Task<CommitResponse> ConfirmarTransaccion(string token)
        {
            try
            {
                var response = await Task.Run(() => _transaction.Commit(token));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar la transacción de WebPay");
                throw;
            }
        }

        public async Task<StatusResponse> ObtenerEstadoTransaccion(string token)
        {
            try
            {
                var response = await Task.Run(() => _transaction.Status(token));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de la transacción de WebPay");
                throw;
            }
        }
    }
} 