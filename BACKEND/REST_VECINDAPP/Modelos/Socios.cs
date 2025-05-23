namespace REST_VECINDAPP.Modelos
{
    public class Socio
    {
        public int idsocio { get; set; }
        public int num_socio { get; set; }
        public int rut { get; set; }
        public DateTime fecha_solicitud { get; set; } = DateTime.Now;
        public DateTime? fecha_aprobacion { get; set; }
        public string? estado_solicitud { get; set; }
        public string? motivo_rechazo { get; set; }
        public string? documento_identidad { get; set; } 
        public string? documento_domicilio { get; set; }
        public int estado { get; set; } = 1;
        public string? motivo_desactivacion { get; set; }
        public DateTime? fecha_desactivacion { get; set; }
    }
}







 