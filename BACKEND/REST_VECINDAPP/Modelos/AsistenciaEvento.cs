namespace REST_VECINDAPP.Modelos
{
    // Modelos/AsistenciaEvento.cs
    public class AsistenciaEvento
    {
        public int Id { get; set; }
        public int EventoId { get; set; }
        public int UsuarioRut { get; set; }
        public DateTime FechaAsistencia { get; set; }
    }
}
