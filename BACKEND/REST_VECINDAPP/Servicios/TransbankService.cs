using Transbank.Webpay.Common;
using Transbank.Webpay.WebpayPlus;
using Transbank.Webpay.WebpayPlus.Responses;
using Microsoft.Extensions.Configuration;
using Transbank.Common;

namespace REST_VECINDAPP.Servicios
{
    public class TransbankService
    {
        private readonly string _commerceCode;
        private readonly string _apiKey;
        private readonly string _returnUrl;
        private readonly string _finalUrl;

        public TransbankService(IConfiguration configuration)
        {
            // Configuración para ambiente de integración
            _commerceCode = "597055555532"; // Código de comercio de prueba
            _apiKey = "579B532A7440BB0C9079DED94D31EA1615BACEB56610332264630D42D0A36B1C";
            _returnUrl = configuration["Transbank:ReturnUrl"] ?? "http://localhost:3000/payment/return";
            _finalUrl = configuration["Transbank:FinalUrl"] ?? "http://localhost:3000/payment/final";
            // No se requiere configuración adicional para integración en el SDK actual
        }

        public async Task<CreateResponse> CreateTransaction(decimal amount, string buyOrder, string sessionId)
        {
            try
            {
                var options = new Options(_commerceCode, _apiKey, WebpayIntegrationType.Test);
                var transaction = new Transaction(options);
                var response = await Task.Run(() => transaction.Create(
                    buyOrder,
                    sessionId,
                    amount,
                    _returnUrl
                ));
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear la transacción: {ex.Message}");
            }
        }

        public async Task<CommitResponse> CommitTransaction(string token)
        {
            try
            {
                var options = new Options(_commerceCode, _apiKey, WebpayIntegrationType.Test);
                var transaction = new Transaction(options);
                var response = await Task.Run(() => transaction.Commit(token));
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al confirmar la transacción: {ex.Message}");
            }
        }

        public async Task<StatusResponse> GetTransactionStatus(string token)
        {
            try
            {
                var options = new Options(_commerceCode, _apiKey, WebpayIntegrationType.Test);
                var transaction = new Transaction(options);
                var response = await Task.Run(() => transaction.Status(token));
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el estado de la transacción: {ex.Message}");
            }
        }
    }
} 