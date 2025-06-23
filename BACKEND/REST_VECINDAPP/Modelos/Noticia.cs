using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos
{
    public class Noticia
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Contenido { get; set; } = string.Empty;
        
        public string? Resumen { get; set; } // Resumen generado automáticamente
        
        public DateTime FechaCreacion { get; set; }
        
        public DateTime? FechaPublicacion { get; set; }
        
        [Required]
        public int AutorRut { get; set; }
        
        public string AutorNombre { get; set; } = string.Empty;
        
        [Required]
        public string Alcance { get; set; } = string.Empty; // PUBLICO, SOCIOS
        
        [Required]
        public string Prioridad { get; set; } = string.Empty; // BAJA, MEDIA, ALTA
        
        [Required]
        public string Estado { get; set; } = string.Empty; // ACTIVO, INACTIVO
        
        public string? ImagenUrl { get; set; }
        
        [Required]
        public string Categoria { get; set; } = string.Empty; // NOTICIA, EVENTO, AVISO
        
        public string? Tags { get; set; } // JSON array como string
    }
} 