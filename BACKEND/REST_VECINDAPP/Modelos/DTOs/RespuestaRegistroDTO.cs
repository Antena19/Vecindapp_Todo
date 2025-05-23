namespace REST_VECINDAPP.Modelos.DTOs
{
    public class RespuestaRegistroDTO
    {
        public int TotalRegistros { get; set; }
        public List<RegistroActividadDTO> Registros { get; set; }
    }
}
