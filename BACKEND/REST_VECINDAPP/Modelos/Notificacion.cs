using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos
{
    public class Notificacion
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Mensaje { get; set; } = string.Empty;
        
        public DateTime FechaCreacion { get; set; }
        
        public DateTime? FechaEnvio { get; set; }
        
        [Required]
        public string Tipo { get; set; } = string.Empty; // PUSH, EMAIL, SMS
        
        [Required]
        public string Estado { get; set; } = string.Empty; // PENDIENTE, ENVIADA, FALLIDA
        
        public string? Destinatarios { get; set; } // JSON array de RUTs como string
        
        public int? NoticiaId { get; set; }
        
        [Required]
        public string Prioridad { get; set; } = string.Empty; // BAJA, MEDIA, ALTA
        
        public string? Metadata { get; set; } // JSON object como string
    }
} 