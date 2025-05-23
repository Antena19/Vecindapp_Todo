using System.Text.Json;
using REST_VECINDAPP.Modelos.DTOs;
using MySql.Data.MySqlClient;
using System.Data;

namespace REST_VECINDAPP.CapaNegocios
{
    public class cn_MercadoPago
    {
        private readonly string _connectionString;
        private readonly string _accessToken;
        private readonly HttpClient _httpClient;
        private const string API_URL = "https://api.mercadopago.com";

        public cn_MercadoPago(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("La cadena de conexión 'DefaultConnection' no está configurada.");
            _accessToken = configuration["MercadoPago:AccessToken"]
                ?? throw new ArgumentException("El token de acceso de MercadoPago no está configurado.");
            
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
        }

        public async Task<PreferenceResponse> CrearPreferenciaPago(PagoCertificadoDTO pago)
        {
            try
            {
                var preference = new
                {
                    items = new[]
                    {
                        new
                        {
                            title = $"Certificado - {pago.TipoCertificado}",
                            quantity = 1,
                            currency_id = "CLP",
                            unit_price = pago.Monto
                        }
                    },
                    back_urls = new
                    {
                        success = $"{pago.UrlBase}/certificados/pago/exito",
                        failure = $"{pago.UrlBase}/certificados/pago/fallo",
                        pending = $"{pago.UrlBase}/certificados/pago/pendiente"
                    },
                    auto_return = "approved",
                    external_reference = pago.SolicitudId.ToString()
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(preference),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{API_URL}/checkout/preferences", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var preferenceResponse = JsonSerializer.Deserialize<PreferenceResponse>(responseContent);

                if (preferenceResponse?.Id != null)
                {
                    GuardarTransaccion(pago.SolicitudId, preferenceResponse.Id, pago.Monto);
                }

                return preferenceResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear la preferencia de pago: {ex.Message}");
            }
        }

        private void GuardarTransaccion(int solicitudId, string preferenciaId, decimal monto)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_REGISTRAR_TRANSACCION_CERTIFICADO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
                    cmd.Parameters.AddWithValue("@p_preferencia_id", preferenciaId);
                    cmd.Parameters.AddWithValue("@p_monto", monto);
                    cmd.Parameters.AddWithValue("@p_estado", "PENDIENTE");

                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        public void ActualizarEstadoPago(string preferenciaId, string estado)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_ACTUALIZAR_ESTADO_PAGO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@p_preferencia_id", preferenciaId);
                    cmd.Parameters.AddWithValue("@p_estado", estado);

                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }
    }

    public class PreferenceResponse
    {
        public string Id { get; set; }
        public string InitPoint { get; set; }
        public string SandboxInitPoint { get; set; }
    }
}
