using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos.DTOs
{
    public class SolicitudCertificadoDTO
    {
        public int Id { get; set; }
        public int UsuarioRut { get; set; }
        public int TipoCertificadoId { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string Estado { get; set; }
        public string Motivo { get; set; }
        public string DocumentosAdjuntos { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public int? DirectivaRut { get; set; }
        public decimal Precio { get; set; }
        public string Observaciones { get; set; }
    }
}
