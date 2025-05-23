namespace REST_VECINDAPP.Modelos
{
    public class Certificado
    {
        public int id { get; set; }
        public int solicitud_id { get; set; }
        public string codigo_verificacion { get; set; }
        public DateTime fecha_emision { get; set; }
        public DateTime? fecha_vencimiento { get; set; }
        public string archivo_pdf { get; set; }
        public string estado { get; set; }

        // Relaciones
        public virtual SolicitudCertificado solicitud { get; set; }
    }
}