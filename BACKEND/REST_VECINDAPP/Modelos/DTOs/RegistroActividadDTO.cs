namespace REST_VECINDAPP.Modelos.DTOs
{
    public class RegistroActividadDTO
    {
        public int Id { get; set; }
        public int? UsuarioRut { get; set; }
        public string NombreUsuario { get; set; }
        public string Accion { get; set; }
        public string Entidad { get; set; }
        public int? EntidadId { get; set; }
        public string Detalles { get; set; }
        public string IP { get; set; }
        public DateTime FechaHora { get; set; }
    }
}
