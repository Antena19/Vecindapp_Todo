using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos.DTOs
{
    public class PagoCertificadoDTO
    {
        [Required(ErrorMessage = "El ID de la solicitud es obligatorio")]
        public int SolicitudId { get; set; }

        [Required(ErrorMessage = "El tipo de certificado es obligatorio")]
        public string TipoCertificado { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "La URL base es obligatoria")]
        public string UrlBase { get; set; }

        // Nueva propiedad para identificar la preferencia de pago
        public string? PreferenciaId { get; set; }
    }
} 