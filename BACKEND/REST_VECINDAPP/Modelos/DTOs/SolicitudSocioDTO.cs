namespace REST_VECINDAPP.Modelos.DTOs
{
    public class SolicitudSocioDTO
    {
        public int Rut { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string EstadoSolicitud { get; set; }
        public string MotivoRechazo { get; set; }
        public string? DocumentoIdentidad { get; set; }
        public string? DocumentoDomicilio { get; set; }
    }
}