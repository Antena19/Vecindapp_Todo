namespace REST_VECINDAPP.Modelos
{
    public class SolicitudCertificado
    {
        public int id { get; set; }
        public int usuario_rut { get; set; }
        public int tipo_certificado_id { get; set; }
        public DateTime fecha_solicitud { get; set; }
        public string estado { get; set; }
        public string motivo { get; set; }
        public string documentos_adjuntos { get; set; }
        public DateTime? fecha_aprobacion { get; set; }
        public int? directiva_rut { get; set; }
        public decimal precio { get; set; }
        public string observaciones { get; set; }
        public int TipoCertificadoId { get; set; }

        // Relaciones
        public virtual Usuario usuario { get; set; }
        public virtual TipoCertificado tipo_certificado { get; set; }
        public virtual Usuario directiva { get; set; }
    }
}