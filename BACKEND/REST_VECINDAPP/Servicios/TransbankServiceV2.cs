using Transbank.Webpay.Common;
using Transbank.Webpay.WebpayPlus;
using Transbank.Webpay.WebpayPlus.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using REST_VECINDAPP.CapaNegocios;
using REST_VECINDAPP.Modelos;
using Transbank.Common;

namespace REST_VECINDAPP.Servicios
{
    public class TransbankServiceV2
    {
        private readonly string _commerceCode;
        private readonly string _apiKey;
        private readonly string _returnUrl;
        private readonly ILogger<TransbankServiceV2> _logger;
        private readonly cn_SolicitudesCertificado _solicitudesService;
        private readonly IConfiguration _configuration;

        public TransbankServiceV2(IConfiguration configuration, ILogger<TransbankServiceV2> logger, cn_SolicitudesCertificado solicitudesService)
        {
            _configuration = configuration;
            _logger = logger;
            _solicitudesService = solicitudesService;
            
            // Configuración simple y directa
            _commerceCode = "597055555532"; // Código de comercio de prueba
            _apiKey = "579B532A7440BB0C9079DED94D31EA1615BACEB56610332264630D42D0A36B1C"; // API Key de prueba
            
            // URL de retorno
            var baseUrl = configuration["Transbank:BaseUrl"] ?? "http://localhost:4200";
            _returnUrl = $"{baseUrl.TrimEnd('/')}/api/Webpay/commit";
            
            _logger.LogInformation($"TransbankServiceV2 inicializado - CommerceCode: {_commerceCode}, ReturnUrl: {_returnUrl}");
        }

        public async Task<CreateResponse> CreateTransaction(decimal amount, string buyOrder, string sessionId)
        {
            try
            {
                _logger.LogInformation($"Creando transacción - Amount: {amount}, BuyOrder: {buyOrder}, SessionId: {sessionId}");
                
                // Configuración correcta usando Options
                var options = new Options(
                    _configuration["Transbank:CommerceCode"],
                    _configuration["Transbank:ApiKey"],
                    WebpayIntegrationType.Test
                );
                var transaction = new Transaction(options);
                
                // Crear la transacción
                var response = transaction.Create(buyOrder, sessionId, amount, _returnUrl);
                
                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    _logger.LogInformation($"Transacción creada exitosamente - Token: {response.Token}, URL: {response.Url}");
                    return response;
                }
                else
                {
                    throw new Exception("La respuesta de Transbank no contiene token válido");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear transacción con Transbank");
                throw;
            }
        }

        public async Task<CommitResponse> CommitTransaction(string token)
        {
            try
            {
                _logger.LogInformation($"Confirmando transacción - Token: {token}");
                
                // Configuración correcta usando Options
                var options = new Options(
                    _configuration["Transbank:CommerceCode"],
                    _configuration["Transbank:ApiKey"],
                    WebpayIntegrationType.Test
                );
                var transaction = new Transaction(options);
                
                var response = transaction.Commit(token);
                
                _logger.LogInformation($"Transacción confirmada - Status: {response.Status}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar transacción con Transbank");
                throw;
            }
        }

        public async Task<StatusResponse> GetTransactionStatus(string token)
        {
            try
            {
                _logger.LogInformation($"Consultando estado de transacción - Token: {token}");
                
                // Configuración correcta usando Options
                var options = new Options(
                    _configuration["Transbank:CommerceCode"],
                    _configuration["Transbank:ApiKey"],
                    WebpayIntegrationType.Test
                );
                var transaction = new Transaction(options);
                
                var response = transaction.Status(token);
                
                _logger.LogInformation($"Estado de transacción - Status: {response.Status}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar estado de transacción con Transbank");
                throw;
            }
        }

        // Método para crear transacción simulada (fallback)
        private CreateResponse CreateSimulatedTransaction(decimal amount, string buyOrder, string sessionId)
        {
            var token = $"simulated_{Guid.NewGuid():N}";
            var url = $"http://localhost:4200/payment/final?token_ws={token}";
            _logger.LogInformation($"Transacción simulada creada - Token: {token}, URL: {url}");
            return new SimulatedCreateResponse(token, url);
        }
    }
} 