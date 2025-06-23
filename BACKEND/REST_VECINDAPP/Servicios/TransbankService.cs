using Transbank.Webpay.Common;
using Transbank.Webpay.WebpayPlus;
using Transbank.Webpay.WebpayPlus.Responses;
using Microsoft.Extensions.Configuration;
using Transbank.Common;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using REST_VECINDAPP.CapaNegocios;
using REST_VECINDAPP.Modelos;

namespace REST_VECINDAPP.Servicios
{
    // Clase personalizada para transacciones simuladas
    public class SimulatedCreateResponse : CreateResponse
    {
        public SimulatedCreateResponse(string token, string url)
        {
            // Usar reflexión para establecer las propiedades de solo lectura
            var tokenProperty = typeof(CreateResponse).GetProperty("Token");
            var urlProperty = typeof(CreateResponse).GetProperty("Url");
            
            if (tokenProperty != null && tokenProperty.CanWrite)
                tokenProperty.SetValue(this, token);
            if (urlProperty != null && urlProperty.CanWrite)
                urlProperty.SetValue(this, url);
        }
    }

    public class TransbankService
    {
        private readonly string _commerceCode;
        private readonly string _apiKey;
        private readonly WebpayIntegrationType _integrationType;
        private readonly string _returnUrl;
        private readonly string _finalUrl;
        private readonly ILogger<TransbankService> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _maxRetries;
        private readonly int _retryDelayMs;
        private readonly HttpClient _httpClient;
        private readonly bool _useSystemProxy;
        private readonly string _customProxyUrl;
        private readonly bool _bypassSslValidation;
        private readonly cn_SolicitudesCertificado _solicitudesService;

        public TransbankService(IConfiguration configuration, ILogger<TransbankService> logger, cn_SolicitudesCertificado solicitudesService)
        {
            _configuration = configuration;
            _logger = logger;
            _solicitudesService = solicitudesService;
            
            // Configuración para ambiente de integración
            _commerceCode = configuration["Transbank:CommerceCode"] ?? "597055555532";
            _apiKey = configuration["Transbank:ApiKey"] ?? "579B532A7440BB0C9079DED94D31EA1615BACEB56610332264630D42D0A36B1C";
            
            // Leer y parsear el tipo de integración para que no esté hardcodeado
            var integrationTypeString = configuration["Transbank:IntegrationType"] ?? "TEST";
            if (integrationTypeString.Equals("LIVE", StringComparison.OrdinalIgnoreCase))
            {
                _integrationType = WebpayIntegrationType.Live;
            }
            else
            {
                _integrationType = WebpayIntegrationType.Test;
            }
            
            // Obtener la URL base del entorno actual
            var baseUrl = GetBaseUrl();
            _returnUrl = $"{baseUrl}/api/certificados/pago/confirmar";
            _finalUrl = $"{baseUrl}/payment/final";
            
            // Configuración de reintentos
            _maxRetries = int.Parse(configuration["Transbank:MaxRetries"] ?? "5");
            _retryDelayMs = int.Parse(configuration["Transbank:RetryDelayMs"] ?? "3000");
            
            // Configuración de proxy y SSL
            _useSystemProxy = configuration.GetValue<bool>("Transbank:UseSystemProxy", true);
            _customProxyUrl = configuration["Transbank:ProxyUrl"] ?? "";
            _bypassSslValidation = configuration.GetValue<bool>("Transbank:BypassSslValidation", false);
            
            // Configurar HttpClient con mejores opciones
            _httpClient = CreateHttpClient();
            
            _logger.LogInformation($"TransbankService inicializado - CommerceCode: {_commerceCode}, ReturnUrl: {_returnUrl}, MaxRetries: {_maxRetries}, UseSystemProxy: {_useSystemProxy}");
        }

        private HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            
            // Configurar proxy
            ConfigureProxy(handler);
            
            // Configurar certificados SSL
            ConfigureSslValidation(handler);
            
            var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromMinutes(5);
            
            // Configurar headers adicionales
            client.DefaultRequestHeaders.Add("User-Agent", "VecindApp/1.0");
            
            return client;
        }

        private void ConfigureProxy(HttpClientHandler handler)
        {
            if (_useSystemProxy)
            {
                try
                {
                    // Usar proxy del sistema automáticamente
                    handler.UseProxy = true;
                    handler.Proxy = WebRequest.GetSystemWebProxy();
                    _logger.LogInformation("Proxy del sistema habilitado automáticamente");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error al configurar proxy del sistema: {ex.Message}");
                    handler.UseProxy = false;
                }
            }
            else if (!string.IsNullOrEmpty(_customProxyUrl) && _customProxyUrl != "auto")
            {
                try
                {
                    // Usar proxy específico configurado
                    handler.Proxy = new WebProxy(_customProxyUrl);
                    handler.UseProxy = true;
                    _logger.LogInformation($"Proxy configurado manualmente: {_customProxyUrl}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error al configurar proxy personalizado: {ex.Message}");
                    handler.UseProxy = false;
                }
            }
            else
            {
                // No usar proxy
                handler.UseProxy = false;
                _logger.LogInformation("Proxy deshabilitado");
            }
        }

        private void ConfigureSslValidation(HttpClientHandler handler)
        {
            if (_bypassSslValidation || _configuration["ASPNETCORE_ENVIRONMENT"] == "Development")
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    _logger.LogWarning($"SSL Validation bypassed - Policy Errors: {sslPolicyErrors}");
                    return true;
                };
                _logger.LogInformation("Validación SSL deshabilitada");
            }
            else
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    if (sslPolicyErrors != SslPolicyErrors.None)
                    {
                        _logger.LogWarning($"SSL Policy Errors: {sslPolicyErrors}");
                    }
                    return sslPolicyErrors == SslPolicyErrors.None;
                };
            }
        }

        private string GetBaseUrl()
        {
            // Detectar si estamos en Railway o local
            var railwayUrl = Environment.GetEnvironmentVariable("RAILWAY_STATIC_URL");
            if (!string.IsNullOrEmpty(railwayUrl))
            {
                // Asegurar que no termine con slash para evitar duplicación
                return railwayUrl.TrimEnd('/');
            }
            
            // Usar la configuración BaseUrl si está disponible
            var baseUrl = _configuration["Transbank:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                return baseUrl.TrimEnd('/');
            }
            
            // Si no hay BaseUrl configurada, extraer de ReturnUrl
            var configuredUrl = _configuration["Transbank:ReturnUrl"] ?? "http://localhost:4200";
            
            // Remover cualquier endpoint específico para obtener solo la URL base
            if (configuredUrl.Contains("/api/certificados/pago/confirmar"))
            {
                configuredUrl = configuredUrl.Replace("/api/certificados/pago/confirmar", "");
            }
            else if (configuredUrl.Contains("/payment/return"))
            {
                configuredUrl = configuredUrl.Replace("/payment/return", "");
            }
            
            return configuredUrl.TrimEnd('/');
        }

        public async Task<CreateResponse> CreateTransaction(decimal amount, string buyOrder, string sessionId)
        {
            // Forzar la lectura desde la sección específica para evitar overrides
            var transbankConfig = _configuration.GetSection("Transbank");
            var enableSimulated = transbankConfig.GetValue<bool>("EnableSimulatedTransactions");

            // Si las transacciones simuladas están habilitadas, usar directamente la simulación
            if (enableSimulated)
            {
                _logger.LogInformation("Usando transacción simulada (modo simulado habilitado por configuración)");
                return CreateSimulatedTransaction(amount, buyOrder, sessionId);
            }

            var retryCount = 0;
            Exception lastException = null;

            while (retryCount <= _maxRetries)
            {
                try
                {
                    _logger.LogInformation($"Iniciando transacción (intento {retryCount + 1}/{_maxRetries + 1}) - Amount: {amount}, BuyOrder: {buyOrder}, SessionId: {sessionId}");
                    
                    // Verificar conectividad antes de intentar la transacción
                    if (retryCount == 0)
                    {
                        var canConnect = await TestConnectivity();
                        if (!canConnect)
                        {
                            _logger.LogWarning("No se puede conectar con Transbank. Verificando conectividad de red...");
                            await TestNetworkConnectivity();
                            
                            // Si es el primer intento y no hay conectividad, probar con diferentes configuraciones
                            if (retryCount == 0)
                            {
                                _logger.LogInformation("Intentando con configuración alternativa...");
                                await TryAlternativeConfiguration();
                            }
                        }
                    }
                    
                    var options = new Options(_commerceCode, _apiKey, _integrationType);
                    var transaction = new Transaction(options);
            
                    // La llamada a Create es síncrona, la envolvemos en Task.Run para usarla en un método async
                    var response = await Task.Run(() => transaction.Create(buyOrder, sessionId, amount, _returnUrl));

                    _logger.LogInformation($"Transacción creada exitosamente - Token: {response.Token}, URL: {response.Url}");
                    return response;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogError(ex, $"Error al crear la transacción (intento {retryCount + 1})");
                    retryCount++;
                    if (retryCount <= _maxRetries)
                    {
                        _logger.LogInformation($"Reintentando en {_retryDelayMs}ms...");
                        await Task.Delay(_retryDelayMs);
                    }
                }
            }

            throw new Exception("No se pudo crear la transacción después de múltiples intentos.", lastException);
        }

        private CreateResponse CreateSimulatedTransaction(decimal amount, string buyOrder, string sessionId)
        {
            var simulatedToken = $"simulated_{Guid.NewGuid():N}";
            var simulatedUrl = $"{_finalUrl}?token_ws={simulatedToken}";
            
            _logger.LogInformation($"Transacción simulada creada - Token: {simulatedToken}, URL: {simulatedUrl}");
            
            return new SimulatedCreateResponse(simulatedToken, simulatedUrl);
        }

        public async Task<CommitResponse> CommitTransaction(string token)
        {
            // Si es una transacción simulada o las simulaciones están habilitadas, simular la confirmación
            if (token.StartsWith("simulated_") || _configuration.GetValue<bool>("Transbank:EnableSimulatedTransactions", false))
            {
                _logger.LogInformation($"Confirmando transacción simulada - Token: {token}");
                
                return new CommitResponse
                {
                    Status = "AUTHORIZED",
                    BuyOrder = "simulated_order",
                    SessionId = "simulated_session",
                    Amount = 0,
                    ResponseCode = 0,
                    AuthorizationCode = "simulated_auth",
                    CardDetail = new CardDetail
                    {
                        CardNumber = "XXXX-XXXX-XXXX-0000"
                    }
                };
            }
            
            var retryCount = 0;
            Exception lastException = null;

            while (retryCount <= _maxRetries)
            {
                try
                {
                    _logger.LogInformation($"Confirmando transacción (intento {retryCount + 1}/{_maxRetries + 1}) - Token: {token}");
                    
                    var options = new Options(_commerceCode, _apiKey, _integrationType);
                    var transaction = new Transaction(options);
                    var response = transaction.Commit(token);
                    
                    _logger.LogInformation($"Transacción confirmada - Status: {response.Status}, BuyOrder: {response.BuyOrder}");
                    return response;
                }
                catch (TaskCanceledException ex)
                {
                    lastException = ex;
                    _logger.LogWarning($"Timeout al confirmar la transacción (intento {retryCount + 1}): {ex.Message}");
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    _logger.LogWarning($"Error de conexión HTTP al confirmar (intento {retryCount + 1}): {ex.Message}");
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning($"Error inesperado al confirmar (intento {retryCount + 1}): {ex.Message}");
                }

                retryCount++;
                
                if (retryCount <= _maxRetries)
                {
                    _logger.LogInformation($"Esperando {_retryDelayMs}ms antes del siguiente intento...");
                    await Task.Delay(_retryDelayMs);
                }
            }

            // Si llegamos aquí, todos los reintentos fallaron
            _logger.LogError($"Todos los intentos de confirmación fallaron después de {_maxRetries + 1} intentos");
            
            if (lastException is TaskCanceledException)
            {
                throw new Exception("Timeout al confirmar la transacción con Transbank después de múltiples intentos. Por favor, intente nuevamente más tarde.");
            }
            else if (lastException is HttpRequestException)
            {
                throw new Exception("Error de conexión al confirmar la transacción después de múltiples intentos. Verifique su conexión a internet e intente nuevamente.");
            }
            else
            {
                throw new Exception($"Error al confirmar la transacción después de múltiples intentos: {lastException?.Message}");
            }
        }

        public async Task<StatusResponse> GetTransactionStatus(string token)
        {
            // Si es una transacción simulada o las simulaciones están habilitadas, simular el estado
            if (token.StartsWith("simulated_") || _configuration.GetValue<bool>("Transbank:EnableSimulatedTransactions", false))
            {
                _logger.LogInformation($"Consultando estado de transacción simulada - Token: {token}");
                
                return new StatusResponse
                {
                    Status = "AUTHORIZED",
                    BuyOrder = "simulated_order",
                    SessionId = "simulated_session",
                    Amount = 0,
                    ResponseCode = 0,
                    AuthorizationCode = "simulated_auth",
                    CardDetail = new CardDetail
                    {
                        CardNumber = "XXXX-XXXX-XXXX-0000"
                    }
                };
            }

            var retryCount = 0;
            Exception lastException = null;

            while (retryCount <= _maxRetries)
            {
                try
                {
                    _logger.LogInformation($"Consultando estado de transacción (intento {retryCount + 1}/{_maxRetries + 1}) - Token: {token}");
                    
                    var options = new Options(_commerceCode, _apiKey, _integrationType);
                    var transaction = new Transaction(options);
                    var response = transaction.Status(token);
                    
                    _logger.LogInformation($"Estado de transacción obtenido - Status: {response.Status}");
                    return response;
                }
                catch (TaskCanceledException ex)
                {
                    lastException = ex;
                    _logger.LogWarning($"Timeout al consultar el estado (intento {retryCount + 1}): {ex.Message}");
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    _logger.LogWarning($"Error de conexión HTTP al consultar estado (intento {retryCount + 1}): {ex.Message}");
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning($"Error inesperado al consultar estado (intento {retryCount + 1}): {ex.Message}");
                }

                retryCount++;
                
                if (retryCount <= _maxRetries)
                {
                    _logger.LogInformation($"Esperando {_retryDelayMs}ms antes del siguiente intento...");
                    await Task.Delay(_retryDelayMs);
                }
            }

            // Si llegamos aquí, todos los reintentos fallaron
            _logger.LogError($"Todos los intentos de consulta de estado fallaron después de {_maxRetries + 1} intentos");
            
            if (lastException is TaskCanceledException)
            {
                throw new Exception("Timeout al consultar el estado de la transacción después de múltiples intentos. Por favor, intente nuevamente más tarde.");
            }
            else if (lastException is HttpRequestException)
            {
                throw new Exception("Error de conexión al consultar el estado de la transacción después de múltiples intentos. Verifique su conexión a internet e intente nuevamente.");
            }
            else
            {
                throw new Exception($"Error al obtener el estado de la transacción después de múltiples intentos: {lastException?.Message}");
            }
        }

        public async Task<bool> TestConnectivity()
        {
            try
            {
                _logger.LogInformation("Probando conectividad con Transbank...");
                
                // Lista de URLs de Transbank para probar (en orden de prioridad)
                var transbankUrls = new[]
                {
                    "https://webpay3gint.transbank.cl/webpayplus/init",
                    "https://webpay3g.transbank.cl/webpayplus/init",
                    "https://webpay3gint.transbank.cl",
                    "https://webpay3g.transbank.cl",
                    "https://webpay.transbank.cl"
                };
                
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                foreach (var url in transbankUrls)
                {
                    try
                    {
                        _logger.LogInformation($"Probando URL: {url}");
                        
                        // Crear un HttpClient específico para esta prueba
                        using var testHandler = new HttpClientHandler();
                        ConfigureSslValidation(testHandler);
                        ConfigureProxy(testHandler);
                        
                        using var testClient = new HttpClient(testHandler);
                        testClient.Timeout = TimeSpan.FromSeconds(15);
                        testClient.DefaultRequestHeaders.Add("User-Agent", "VecindApp/1.0");
                        
                        var response = await testClient.GetAsync(url, cts.Token);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation($"Conectividad con Transbank exitosa usando: {url}");
                            return true;
                        }
                        else
                        {
                            _logger.LogWarning($"Transbank respondió con código: {response.StatusCode} para URL: {url}");
                            // Para endpoints de API, un 404 puede ser normal en el endpoint raíz
                            if (response.StatusCode == HttpStatusCode.NotFound && !url.EndsWith("/init"))
                            {
                                _logger.LogInformation("404 en endpoint raíz es normal, continuando con otras URLs...");
                                continue;
                            }
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogWarning($"Error de conexión HTTP para {url}: {ex.Message}");
                    }
                    catch (TaskCanceledException ex)
                    {
                        _logger.LogWarning($"Timeout para {url}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error inesperado para {url}: {ex.Message}");
                    }
                }
                
                // Si ninguna URL funciona, probar conectividad básica a internet
                _logger.LogInformation("Probando conectividad básica a internet...");
                try
                {
                    using var testHandler = new HttpClientHandler();
                    ConfigureSslValidation(testHandler);
                    ConfigureProxy(testHandler);
                    
                    using var testClient = new HttpClient(testHandler);
                    testClient.Timeout = TimeSpan.FromSeconds(10);
                    
                    var internetResponse = await testClient.GetAsync("https://www.google.com", cts.Token);
                    if (internetResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Conectividad a internet OK, pero Transbank no responde. Posible problema con Transbank o firewall.");
                        
                        // Probar con DNS específico
                        try
                        {
                            var hostEntry = await Dns.GetHostEntryAsync("webpay3gint.transbank.cl");
                            _logger.LogInformation($"DNS resuelto correctamente: {string.Join(", ", hostEntry.AddressList.Select(ip => ip.ToString()))}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error de resolución DNS: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Sin conectividad a internet: {ex.Message}");
                }
                
                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError($"Timeout al probar conectividad con Transbank: {ex.Message}");
                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error de conexión HTTP con Transbank: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado al probar conectividad con Transbank: {ex.Message}");
                return false;
            }
        }

        public async Task TestNetworkConnectivity()
        {
            _logger.LogInformation("=== Diagnóstico de Red ===");
            
            // Probar DNS
            try
            {
                var hostEntry = await Dns.GetHostEntryAsync("webpay3gint.transbank.cl");
                _logger.LogInformation($"DNS resuelto correctamente: {string.Join(", ", hostEntry.AddressList.Select(ip => ip.ToString()))}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error de resolución DNS: {ex.Message}");
            }
            
            // Probar conectividad básica a internet
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = await _httpClient.GetAsync("https://www.google.com", cts.Token);
                _logger.LogInformation($"Conectividad a internet: {(response.IsSuccessStatusCode ? "OK" : "Error")}");
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Timeout al conectar a internet");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error de conectividad a internet: {ex.Message}");
            }
            
            // Probar puerto 443 específicamente
            try
            {
                using var tcpClient = new System.Net.Sockets.TcpClient();
                var connectTask = tcpClient.ConnectAsync("webpay3gint.transbank.cl", 443);
                if (await Task.WhenAny(connectTask, Task.Delay(10000)) == connectTask)
                {
                    _logger.LogInformation("Puerto 443 accesible");
                    tcpClient.Close();
                }
                else
                {
                    _logger.LogError("Timeout al conectar al puerto 443");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al conectar al puerto 443: {ex.Message}");
            }
            
            // Probar conectividad HTTPS específica
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                var response = await _httpClient.GetAsync("https://webpay3gint.transbank.cl", cts.Token);
                _logger.LogInformation($"Conectividad HTTPS a Transbank: {(response.IsSuccessStatusCode ? "OK" : $"Error {response.StatusCode}")}");
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Timeout al conectar HTTPS a Transbank");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error de conectividad HTTPS a Transbank: {ex.Message}");
            }
            
            _logger.LogInformation("=== Fin Diagnóstico de Red ===");
        }

        public async Task<Dictionary<string, object>> GetDiagnosticInfo()
        {
            var diagnostic = new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.Now,
                ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                ["railwayUrl"] = Environment.GetEnvironmentVariable("RAILWAY_STATIC_URL"),
                ["port"] = Environment.GetEnvironmentVariable("PORT"),
                ["transbankConfig"] = new Dictionary<string, object>
                {
                    ["commerceCode"] = _commerceCode,
                    ["returnUrl"] = _returnUrl,
                    ["maxRetries"] = _maxRetries,
                    ["retryDelayMs"] = _retryDelayMs,
                    ["proxyUrl"] = _configuration["Transbank:ProxyUrl"] ?? "No configurado"
                },
                ["connectivity"] = new Dictionary<string, object>
                {
                    ["canReachTransbank"] = await TestConnectivity(),
                    ["error"] = ""
                }
            };

            return diagnostic;
        }

        public async Task<bool> TestConnectivityWithoutProxy()
        {
            try
            {
                _logger.LogInformation("Probando conectividad sin proxy...");
                
                var handler = new HttpClientHandler();
                handler.UseProxy = false;
                ConfigureSslValidation(handler);
                
                var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromMinutes(2);
                
                var response = await client.GetAsync("https://webpay3gint.transbank.cl");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Conectividad sin proxy exitosa");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Transbank respondió sin proxy con código: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al probar conectividad sin proxy: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> TestConnectivityWithExtendedTimeout()
        {
            try
            {
                _logger.LogInformation("Probando conectividad con timeout extendido...");
                
                var handler = new HttpClientHandler();
                ConfigureProxy(handler);
                ConfigureSslValidation(handler);
                
                var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromMinutes(10);
                
                var response = await client.GetAsync("https://webpay3gint.transbank.cl");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Conectividad con timeout extendido exitosa");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Transbank respondió con timeout extendido con código: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al probar conectividad con timeout extendido: {ex.Message}");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> TestPortConnectivity()
        {
            var resultados = new Dictionary<string, object>();
            
            // Probar puerto 443
            try
            {
                using var tcpClient = new System.Net.Sockets.TcpClient();
                var connectTask = tcpClient.ConnectAsync("webpay3gint.transbank.cl", 443);
                if (await Task.WhenAny(connectTask, Task.Delay(10000)) == connectTask)
                {
                    resultados["puerto_443"] = true;
                    tcpClient.Close();
                }
                else
                {
                    resultados["puerto_443"] = false;
                }
            }
            catch (Exception ex)
            {
                resultados["puerto_443"] = false;
                resultados["puerto_443_error"] = ex.Message;
            }
            
            // Probar puerto 80 (HTTP)
            try
            {
                using var tcpClient = new System.Net.Sockets.TcpClient();
                var connectTask = tcpClient.ConnectAsync("webpay3gint.transbank.cl", 80);
                if (await Task.WhenAny(connectTask, Task.Delay(10000)) == connectTask)
                {
                    resultados["puerto_80"] = true;
                    tcpClient.Close();
                }
                else
                {
                    resultados["puerto_80"] = false;
                }
            }
            catch (Exception ex)
            {
                resultados["puerto_80"] = false;
                resultados["puerto_80_error"] = ex.Message;
            }
            
            return resultados;
        }

        private async Task TryAlternativeConfiguration()
        {
            _logger.LogInformation("Probando configuración alternativa...");
            
            var alternativeMethods = _configuration.GetSection("Transbank:AlternativeConnectionMethods").Get<string[]>() ?? 
                                   new[] { "direct", "proxy", "extended_timeout" };
            
            foreach (var method in alternativeMethods)
            {
                _logger.LogInformation($"Probando método: {method}");
                
                switch (method.ToLower())
                {
                    case "direct":
                        await TestDirectConnection();
                        break;
                    case "proxy":
                        await TestProxyConnection();
                        break;
                    case "extended_timeout":
                        await TestExtendedTimeoutConnection();
                        break;
                }
            }
            
            // Proporcionar recomendaciones basadas en los resultados
            _logger.LogInformation("=== Recomendaciones ===");
            _logger.LogInformation("Si los problemas persisten, considera:");
            _logger.LogInformation("1. Verificar la conectividad a internet");
            _logger.LogInformation("2. Deshabilitar temporalmente el firewall");
            _logger.LogInformation("3. Usar una VPN si estás en una red corporativa");
            _logger.LogInformation("4. Contactar al administrador de red");
            _logger.LogInformation("5. Usar transacciones simuladas para desarrollo");
        }
        
        private async Task TestDirectConnection()
        {
            try
            {
                _logger.LogInformation("Probando conexión directa sin proxy...");
                var handler = new HttpClientHandler();
                handler.UseProxy = false;
                ConfigureSslValidation(handler);
                
                var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromMinutes(2);
                
                var response = await client.GetAsync("https://webpay3gint.transbank.cl");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("¡Conectividad directa exitosa! Considera deshabilitar el proxy del sistema.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Conexión directa falló: {ex.Message}");
            }
        }
        
        private async Task TestProxyConnection()
        {
            try
            {
                _logger.LogInformation("Probando conexión con proxy del sistema...");
                var handler = new HttpClientHandler();
                handler.UseProxy = true;
                handler.Proxy = WebRequest.GetSystemWebProxy();
                ConfigureSslValidation(handler);
                
                var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromMinutes(2);
                
                var response = await client.GetAsync("https://webpay3gint.transbank.cl");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("¡Conectividad con proxy exitosa!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Conexión con proxy falló: {ex.Message}");
            }
        }
        
        private async Task TestExtendedTimeoutConnection()
        {
            try
            {
                _logger.LogInformation("Probando conexión con timeout extendido...");
                var handler = new HttpClientHandler();
                ConfigureProxy(handler);
                ConfigureSslValidation(handler);
                
                var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromMinutes(10);
                
                var response = await client.GetAsync("https://webpay3gint.transbank.cl");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("¡Conectividad con timeout extendido exitosa!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Conexión con timeout extendido falló: {ex.Message}");
            }
        }

        public async Task<bool> TestInitEndpoint()
        {
            try
            {
                _logger.LogInformation("Probando endpoint /init de Transbank...");
                
                var options = new Options(_commerceCode, _apiKey, _integrationType);
                var transaction = new Transaction(options);
                
                // Crear una transacción de prueba
                var result = transaction.Create(
                    buyOrder: "test-" + DateTime.Now.Ticks,
                    sessionId: "test-session",
                    amount: 1000m,
                    returnUrl: _returnUrl
                );
                
                _logger.LogInformation($"✅ Endpoint /init funciona correctamente. Token: {result.Token}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al probar endpoint /init: {ex.Message}");
                return false;
            }
        }

        // ✅ Método para iniciar transacción con el SDK oficial
        public async Task<CrearTransaccionResultado> IniciarTransaccionSDKAsync(int solicitudId, decimal monto)
        {
            _logger.LogInformation($"Iniciando transacción con SDK para solicitud {solicitudId} y monto {monto}");

            // Si las transacciones simuladas están habilitadas, usar directamente la simulación
            if (_configuration.GetValue<bool>("Transbank:EnableSimulatedTransactions", false))
            {
                _logger.LogInformation("Usando transacción simulada (modo simulado habilitado)");
                
                string simulatedBuyOrder = $"cert-{solicitudId}-{DateTime.Now.Ticks}";
                var simulatedToken = $"simulated_{Guid.NewGuid():N}";
                var simulatedUrl = $"{_finalUrl}?token_ws={simulatedToken}";
                
                _logger.LogInformation($"Transacción simulada creada - Token: {simulatedToken}, URL: {simulatedUrl}");
                
                return new CrearTransaccionResultado
                {
                    Token = simulatedToken,
                    Url = simulatedUrl,
                    Exito = true,
                    Mensaje = "Transacción simulada creada (modo simulado habilitado)"
                };
            }

            try
            {
                // Verificar conectividad con Transbank
                var canConnect = await TestConnectivity();
                if (!canConnect)
                {
                    _logger.LogWarning("Sin conectividad con Transbank. Verificando si las transacciones simuladas están habilitadas...");
                    
                    if (_configuration.GetValue<bool>("Transbank:EnableSimulatedTransactions", false))
                    {
                        _logger.LogInformation("Creando transacción simulada como fallback...");
                        
                        string simulatedBuyOrder = $"cert-{solicitudId}-{DateTime.Now.Ticks}";
                        var simulatedToken = $"simulated_{Guid.NewGuid():N}";
                        var simulatedUrl = $"{_finalUrl}?token_ws={simulatedToken}";
                        
                        _logger.LogInformation($"Transacción simulada creada - Token: {simulatedToken}, URL: {simulatedUrl}");
                        
                        return new CrearTransaccionResultado
                        {
                            Token = simulatedToken,
                            Url = simulatedUrl,
                            Exito = true,
                            Mensaje = "Transacción simulada creada (sin conectividad con Transbank)"
                        };
                    }
                }

                var options = new Options(_commerceCode, _apiKey, _integrationType);
                var transaction = new Transaction(options);

                string buyOrder = $"cert-{solicitudId}-{DateTime.Now.Ticks}";
                string sessionId = solicitudId.ToString();
                string returnUrl = _returnUrl;

                var result = transaction.Create(
                    buyOrder: buyOrder,
                    sessionId: sessionId,
                    amount: monto,
                    returnUrl: returnUrl
                );

                _logger.LogInformation($"Transacción creada exitosamente. Token: {result.Token}, URL: {result.Url}");

                return new CrearTransaccionResultado
                {
                    Token = result.Token,
                    Url = result.Url,
                    Exito = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al iniciar transacción con SDK: {ex.Message}");
                
                // Si hay error y las transacciones simuladas están habilitadas, intentar crear una simulada
                if (_configuration.GetValue<bool>("Transbank:EnableSimulatedTransactions", false))
                {
                    _logger.LogInformation("Intentando crear transacción simulada como fallback debido al error...");
                    
                    string simulatedBuyOrder = $"cert-{solicitudId}-{DateTime.Now.Ticks}";
                    var simulatedToken = $"simulated_{Guid.NewGuid():N}";
                    var simulatedUrl = $"{_finalUrl}?token_ws={simulatedToken}";
                    
                    return new CrearTransaccionResultado
                    {
                        Token = simulatedToken,
                        Url = simulatedUrl,
                        Exito = true,
                        Mensaje = "Transacción simulada creada como fallback"
                    };
                }
                
                return new CrearTransaccionResultado
                {
                    Exito = false,
                    Mensaje = ex.Message
                };
            }
        }

        public async Task GuardarResultadoPago(string token, string estado, object monto, string buyOrder)
        {
            try
            {
                int solicitudId = ExtraerIdDesdeBuyOrder(buyOrder);
                var solicitud = await _solicitudesService.FindAsync(solicitudId);
                if (solicitud != null)
                {
                    bool resultado = await _solicitudesService.ActualizarEstadoPagoAsync(
                        solicitudId, 
                        estado.ToLower() == "authorized" ? "Pagado" : "Rechazado", 
                        token, 
                        Convert.ToInt32(monto ?? 0)
                    );
                    if (resultado)
                    {
                        _logger.LogInformation($"Estado de pago actualizado correctamente para la solicitud {solicitudId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al guardar resultado de pago: {ex.Message}");
            }
        }

        public async Task GuardarPagoEnHistorial(string token, string estado, object monto, string buyOrder)
        {
            try
            {
                int solicitudId = ExtraerIdDesdeBuyOrder(buyOrder);
                await _solicitudesService.GuardarPagoEnHistorialAsync(
                    solicitudId,
                    token,
                    Convert.ToInt32(monto ?? 0),
                    estado
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al guardar pago en historial: {ex.Message}");
            }
        }

        private int ExtraerIdDesdeBuyOrder(string buyOrder)
        {
            // Ejemplo: cert-86-638860567000017294
            var partes = buyOrder.Split("-");
            return int.TryParse(partes.Length > 1 ? partes[1] : "0", out var id) ? id : 0;
        }
    }

    public class CrearTransaccionResultado
    {
        public bool Exito { get; set; }
        public string Token { get; set; }
        public string Url { get; set; }
        public string Mensaje { get; set; }
    }
} 