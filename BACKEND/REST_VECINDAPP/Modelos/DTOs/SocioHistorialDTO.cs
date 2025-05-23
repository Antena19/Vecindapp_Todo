// Modelos/DTOs/SocioHistorialDTO.cs
public class SocioHistorialDTO
{
    public int IdSocio { get; set; }
    public int num_socio { get; set; }
    public int Rut { get; set; }
    public string DvRut { get; set; }
    public string NombreCompleto { get; set; }
    public string Correo { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public int Estado { get; set; }
    public string? MotivoDesactivacion { get; set; }
    public DateTime? FechaDesactivacion { get; set; }
}