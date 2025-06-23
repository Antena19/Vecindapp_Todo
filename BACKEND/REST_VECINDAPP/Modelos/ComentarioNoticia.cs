using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos
{
    public class ComentarioNoticia
    {
        public int Id { get; set; }
        
        [Required]
        public int NoticiaId { get; set; }
        
        [Required]
        public int UsuarioRut { get; set; }
        
        public string UsuarioNombre { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Contenido { get; set; } = string.Empty;
        
        public DateTime FechaCreacion { get; set; }
        
        [Required]
        public string Estado { get; set; } = string.Empty; // ACTIVO, MODERADO, ELIMINADO
    }
} 