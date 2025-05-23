using System;
using System.ComponentModel.DataAnnotations;

namespace REST_VECINDAPP.Modelos
{
    /*
     CREACION DE MODELO USUARIO (REPLICAR TABLAS DE LA BASE DATOS) 
     */
    public class Usuario
    {
        [Required(ErrorMessage = "El RUT es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El RUT debe ser un número válido")]
        public int rut { get; set; }

        [Required(ErrorMessage = "El dígito verificador es obligatorio")]
        [RegularExpression(@"^[0-9kK]$", ErrorMessage = "El dígito verificador debe ser un número o K")]
        public string? dv_rut { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres")]
        public string? nombre { get; set; }

        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El apellido paterno debe tener entre 1 y 100 caracteres")]
        public string? apellido_paterno { get; set; }

        [StringLength(100, ErrorMessage = "El apellido materno no puede exceder los 100 caracteres")]
        public string? apellido_materno { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string? correo_electronico { get; set; }

        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        public string? telefono { get; set; }

        public string? direccion { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        public string? password { get; set; }

        public DateTime fecha_registro { get; set; } = DateTime.Now;
        public int estado { get; set; } = 1;
        public string? tipo_usuario { get; set; }
        public string? token_recuperacion { get; set; }
        public DateTime? fecha_token_recuperacion { get; set; }
    }
}