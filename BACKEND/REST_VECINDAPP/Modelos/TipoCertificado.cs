namespace REST_VECINDAPP.Modelos
{
    public class TipoCertificado
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public decimal precio_socio { get; set; }
        public decimal precio_vecino { get; set; }
        public string documentos_requeridos { get; set; }
        public bool activo { get; set; }
        public string medios_pago_habilitados { get; set; }
    }
}
