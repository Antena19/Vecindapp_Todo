using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos.DTOs
{
    public class MarcarNotificacionLeidaDTO
    {
        [Required]
        public int UsuarioRut { get; set; }
    }
} 