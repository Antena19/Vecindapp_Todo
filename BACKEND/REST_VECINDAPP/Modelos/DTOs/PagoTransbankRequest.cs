using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos.DTOs
{
    public class PagoTransbankRequest
    {
        [Required]
        public int SolicitudId { get; set; }
        [Required]
        public decimal Monto { get; set; }
        [Required]
        public string RutUsuario { get; set; }
        public string? Token { get; set; }
    }
} 