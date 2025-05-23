using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos.DTOs
{
    public class ConfigurarTarifaRequest
    {
        [Required]
        public int TipoCertificadoId { get; set; }

        [Required]
        public decimal PrecioSocio { get; set; }

        [Required]
        public decimal PrecioVecino { get; set; }

        public string MediosPago { get; set; }
    }
}
