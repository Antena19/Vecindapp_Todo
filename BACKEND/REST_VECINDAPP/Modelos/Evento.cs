namespace REST_VECINDAPP.Modelos
{
    // Modelos/Evento.cs
    public class Evento
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaEvento { get; set; }
        public TimeSpan HoraEvento { get; set; }
        public string Lugar { get; set; }
        public int DirectivaRut { get; set; }
        public string Estado { get; set; } = "activo";
        public string CodigoQr { get; set; }
        public string? CodigoNumerico { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Notas { get; set; }
    }
}
