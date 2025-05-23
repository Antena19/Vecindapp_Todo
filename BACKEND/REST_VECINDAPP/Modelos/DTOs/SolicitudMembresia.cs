namespace REST_VECINDAPP.Modelos.DTOs
{
    public class SolicitudMembresia
    {
        public int Rut { get; set; }
        public IFormFile DocumentoIdentidad { get; set; }
        public IFormFile DocumentoDomicilio { get; set; }
    }
}
