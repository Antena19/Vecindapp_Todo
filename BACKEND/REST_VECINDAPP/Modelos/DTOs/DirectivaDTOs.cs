using System;

namespace REST_VECINDAPP.Modelos.DTOs
{
    public class AprobarCertificadoRequest
    {
        public int DirectivaRut { get; set; }
        public string Observaciones { get; set; }
    }

    public class RechazarCertificadoRequest
    {
        public int DirectivaRut { get; set; }
        public string MotivoRechazo { get; set; }
    }

    public class ConfigurarTarifaRequest
    {
        public decimal PrecioSocio { get; set; }
        public decimal PrecioVecino { get; set; }
        public string MediosPago { get; set; }
    }
} 