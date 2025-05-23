
using System.ComponentModel.DataAnnotations;

/// Modelo para la solicitud de recuperación de contraseña simple
public class RecuperacionSimpleRequest
{
    [Required(ErrorMessage = "El RUT es obligatorio")]
    public int Rut { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    public string? NombreCompleto { get; set; }

    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
    public string? NuevaContrasena { get; set; }

    [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
    [Compare("NuevaContrasena", ErrorMessage = "Las contraseñas no coinciden")]
    public string? ConfirmarContrasena { get; set; }
}