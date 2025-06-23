using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos.DTOs
{
    public class CrearNoticiaDTO
    {
        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Contenido { get; set; } = string.Empty;
        
        [Required]
        public string Categoria { get; set; } = string.Empty; // NOTICIA, EVENTO, AVISO
        
        [Required]
        public string Alcance { get; set; } = string.Empty; // PUBLICO, SOCIOS
        
        [Required]
        public string Prioridad { get; set; } = string.Empty; // BAJA, MEDIA, ALTA
        
        public List<string>? Tags { get; set; }
        
        public bool PublicarInmediatamente { get; set; } = false;
    }
} 