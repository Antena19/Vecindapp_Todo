using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos.DTOs
{
    public class CrearNotificacionDTO
    {
        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Mensaje { get; set; } = string.Empty;
        
        [Required]
        public string Tipo { get; set; } = string.Empty; // AVISO, EVENTO, NOTICIA, etc.
        
        [Required]
        public string Prioridad { get; set; } = string.Empty; // BAJA, MEDIA, ALTA
        
        [Required]
        public List<int> Destinatarios { get; set; } = new List<int>();
        
        public int? NoticiaId { get; set; }
    }
} 