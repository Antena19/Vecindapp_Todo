using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using REST_VECINDAPP.Modelos;
using System.Data;
using REST_VECINDAPP.Modelos.DTOs;
using Microsoft.Extensions.Configuration;

namespace REST_VECINDAPP.CapaNegocios
{
    public class cn_Certificados
    {
        private readonly string _connectionString;

        public cn_Certificados(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("La cadena de conexión 'DefaultConnection' no está configurada.");
        }

        public async Task<bool> SolicitarCertificado(int usuarioId, SolicitudCertificadoDTO solicitud)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_SOLICITAR_CERTIFICADO", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_usuario_rut", usuarioId);
            command.Parameters.AddWithValue("@p_tipo_certificado_id", solicitud.TipoCertificadoId);
            command.Parameters.AddWithValue("@p_motivo", solicitud.Motivo);
            command.Parameters.AddWithValue("@p_documentos_adjuntos", solicitud.DocumentosAdjuntos);
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> AprobarCertificado(int solicitudId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_APROBAR_CERTIFICADO", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> RechazarCertificado(int solicitudId, string motivoRechazo)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_RECHAZAR_CERTIFICADO", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
            command.Parameters.AddWithValue("@p_motivo_rechazo", motivoRechazo);
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<List<TipoCertificado>> ObtenerTiposCertificado()
        {
            var tipos = new List<TipoCertificado>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_OBTENER_TIPOS_CERTIFICADO", connection);
            command.CommandType = CommandType.StoredProcedure;
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tipos.Add(new TipoCertificado
                {
                    id = reader.GetInt32("id"),
                    nombre = reader.GetString("nombre"),
                    descripcion = reader.GetString("descripcion"),
                    precio_socio = reader.GetDecimal("precio_socio"),
                    precio_vecino = reader.GetDecimal("precio_vecino"),
                    documentos_requeridos = reader.GetString("documentos_requeridos"),
                    activo = reader.GetBoolean("activo"),
                    medios_pago_habilitados = reader.GetString("medios_pago_habilitados")
                });
            }
            return tipos;
        }

        public async Task<List<SolicitudCertificadoDTO>> ObtenerSolicitudesUsuario(int usuarioRut)
        {
            var solicitudes = new List<SolicitudCertificadoDTO>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_OBTENER_SOLICITUDES_USUARIO", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_usuario_rut", usuarioRut);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                solicitudes.Add(new SolicitudCertificadoDTO
                {
                    Id = reader.GetInt32("id"),
                    UsuarioRut = reader.GetInt32("usuario_rut"),
                    TipoCertificadoId = reader.GetInt32("tipo_certificado_id"),
                    FechaSolicitud = reader.GetDateTime("fecha_solicitud"),
                    Estado = reader.GetString("estado"),
                    Motivo = reader.GetString("motivo"),
                    DocumentosAdjuntos = reader.GetString("documentos_adjuntos"),
                    FechaAprobacion = reader.IsDBNull("fecha_aprobacion") ? null : reader.GetDateTime("fecha_aprobacion"),
                    DirectivaRut = reader.IsDBNull("directiva_rut") ? null : reader.GetInt32("directiva_rut"),
                    Precio = reader.GetDecimal("precio"),
                    Observaciones = reader.GetString("observaciones")
                });
            }
            return solicitudes;
        }

        public async Task<List<SolicitudCertificadoDTO>> ObtenerSolicitudesPendientes()
        {
            var solicitudes = new List<SolicitudCertificadoDTO>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_OBTENER_SOLICITUDES_PENDIENTES", connection);
            command.CommandType = CommandType.StoredProcedure;
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                solicitudes.Add(new SolicitudCertificadoDTO
                {
                    Id = reader.GetInt32("id"),
                    UsuarioRut = reader.GetInt32("usuario_rut"),
                    TipoCertificadoId = reader.GetInt32("tipo_certificado_id"),
                    FechaSolicitud = reader.GetDateTime("fecha_solicitud"),
                    Estado = reader.GetString("estado"),
                    Motivo = reader.IsDBNull("motivo") ? null : reader.GetString("motivo"),
                    DocumentosAdjuntos = reader.IsDBNull("documentos_adjuntos") ? null : reader.GetString("documentos_adjuntos"),
                    FechaAprobacion = reader.IsDBNull("fecha_aprobacion") ? null : reader.GetDateTime("fecha_aprobacion"),
                    DirectivaRut = reader.IsDBNull("directiva_rut") ? null : reader.GetInt32("directiva_rut"),
                    Precio = reader.GetDecimal("precio"),
                    Observaciones = reader.IsDBNull("observaciones") ? null : reader.GetString("observaciones")
                });
            }
            return solicitudes;
        }

        public async Task<SolicitudCertificadoDTO> ObtenerDetalleSolicitud(int solicitudId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_OBTENER_DETALLE_SOLICITUD", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new SolicitudCertificadoDTO
                {
                    Id = reader.GetInt32("id"),
                    UsuarioRut = reader.GetInt32("usuario_rut"),
                    TipoCertificadoId = reader.GetInt32("tipo_certificado_id"),
                    FechaSolicitud = reader.GetDateTime("fecha_solicitud"),
                    Estado = reader.GetString("estado"),
                    Motivo = reader.GetString("motivo"),
                    DocumentosAdjuntos = reader.GetString("documentos_adjuntos"),
                    FechaAprobacion = reader.IsDBNull("fecha_aprobacion") ? null : reader.GetDateTime("fecha_aprobacion"),
                    DirectivaRut = reader.IsDBNull("directiva_rut") ? null : reader.GetInt32("directiva_rut"),
                    Precio = reader.GetDecimal("precio"),
                    Observaciones = reader.GetString("observaciones")
                };
            }
            return null;
        }

        public async Task<string> ObtenerEstadoPago(string preferenciaId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_OBTENER_ESTADO_PAGO", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_preferencia_id", preferenciaId);
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }
    }
} 