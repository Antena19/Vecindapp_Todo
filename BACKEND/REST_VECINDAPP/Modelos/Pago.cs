using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REST_VECINDAPP.Modelos
{
    [Table("pago")]
    public class Pago
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("solicitud_id")]
        public int SolicitudId { get; set; }

        [ForeignKey("SolicitudId")]
        public SolicitudCertificado Solicitud { get; set; }

        [Column("token_webpay")]
        public string TokenWebpay { get; set; }

        [Column("monto")]
        public int? Monto { get; set; }

        [Column("estado")]
        public string Estado { get; set; }

        [Column("fecha_pago")]
        public DateTime? FechaPago { get; set; }
    }
} 