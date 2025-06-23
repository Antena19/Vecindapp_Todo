using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos
{
    public class NotificacionUsuario
    {
        public int Id { get; set; }
        
        [Required]
        public int NotificacionId { get; set; }
        
        [Required]
        public int UsuarioRut { get; set; }
        
        public bool Leida { get; set; }
        
        public DateTime? FechaLectura { get; set; }
        
        public DateTime FechaRecepcion { get; set; }
    }
} 