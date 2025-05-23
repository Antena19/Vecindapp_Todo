// Models/DTOs/ActualizarEstadoSocioDTO.cs
public class ActualizarEstadoSocioDTO
{
    public int Estado { get; set; }  // 1 = activo, 2 = inactivo
    public string? Motivo { get; set; }
}