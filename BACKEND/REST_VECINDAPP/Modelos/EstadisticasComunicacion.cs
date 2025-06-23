namespace REST_VECINDAPP.Modelos
{
    public class EstadisticasComunicacion
    {
        public int TotalNoticias { get; set; }
        public int NoticiasPublicas { get; set; }
        public int NoticiasSocios { get; set; }
        public int NotificacionesEnviadas { get; set; }
        public int NotificacionesPendientes { get; set; }
        public int UsuariosNotificados { get; set; }
        public double TasaLectura { get; set; } // Porcentaje
    }
} 