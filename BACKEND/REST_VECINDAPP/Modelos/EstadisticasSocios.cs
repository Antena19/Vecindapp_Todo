// Models/EstadisticasSocios.cs
public class EstadisticasSocios
{
    public int TotalSocios { get; set; }
    public int SolicitudesPendientes { get; set; }
    public int SociosActivos { get; set; }
    public int SociosInactivos { get; set; }
}

// Models/Actividad.cs
public class Actividad
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public DateTime Fecha { get; set; }
    public string Icono { get; set; }
    public string Color { get; set; }
}