using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos.DTOs
{
    public class CrearComentarioDTO
    {
        [Required]
        [StringLength(500)]
        public string Contenido { get; set; } = string.Empty;
        
        [Required]
        public int UsuarioRut { get; set; }
    }
} 