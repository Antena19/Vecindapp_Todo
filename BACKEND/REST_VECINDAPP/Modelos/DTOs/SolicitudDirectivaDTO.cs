using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos.DTOs
{
    public class AprobarCertificadoRequest
    {
        [Required(ErrorMessage = "El ID de la solicitud es obligatorio")]
        public int SolicitudId { get; set; }

        [Required(ErrorMessage = "El RUT de la directiva es obligatorio")]
        public string DirectivaRut { get; set; }

        public string Observaciones { get; set; }
    }

    public class RechazarCertificadoRequest
    {
        [Required(ErrorMessage = "El ID de la solicitud es obligatorio")]
        public int SolicitudId { get; set; }

        [Required(ErrorMessage = "El RUT de la directiva es obligatorio")]
        public string DirectivaRut { get; set; }

        [Required(ErrorMessage = "El motivo del rechazo es obligatorio")]
        public string MotivoRechazo { get; set; }
    }
} 
